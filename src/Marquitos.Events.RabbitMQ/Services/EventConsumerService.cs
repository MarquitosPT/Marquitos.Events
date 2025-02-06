using EasyNetQ;
using EasyNetQ.Topology;
using Marquitos.Events.RabbitMQ.Consumers;
using Marquitos.Events.RabbitMQ.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Marquitos.Events.RabbitMQ.Services
{
    internal class EventConsumerService<TConsumer, TMessage> : IEventConsumerService, IDisposable where TConsumer : EventConsumer<TMessage> where TMessage : class, IEvent
    {
        private readonly IServiceProvider _serviceProdiver;
        private readonly IHostEnvironment _hostEnvironment;

        private readonly IBus _bus;
        private readonly IConventions _conventions;
        private readonly ILogger<EventConsumerService<TConsumer, TMessage>> _logger;

        private IDisposable subscription;
        private IDisposable managementSubscription;
        private ConsumerOptions options;
        private string subscriptionId;
        private JsonSerializerOptions serializeOptions;

        private Queue consumerQueue;

        private const string RetriesHeaderKey = "x-retries";

        public EventConsumerService(IServiceProvider serviceProdiver, IHostEnvironment hostEnvironment, IBus bus, IConventions conventions, ILogger<EventConsumerService<TConsumer, TMessage>> logger)
        {
            _serviceProdiver = serviceProdiver;
            _hostEnvironment = hostEnvironment;
            _bus = bus;
            _conventions = conventions;
            _logger = logger;

            subscriptionId = _hostEnvironment.ApplicationName;
            options = new ConsumerOptions
            {
                Durable = true,
                AutoDelete = false,
                PrefetchCount = 1
            };

            serializeOptions = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                Converters = {
                    new JsonStringEnumConverter(),
                    new DateOnlyJsonConverter(),
                    new TimeOnlyJsonConverter()
                }
            };

            _logger.BeginScope("{EventConsumer}", typeof(TConsumer).Name);
        }

        public bool IsEnabled { get; protected set; } = false;

        public bool IsConsuming { get; protected set; } = false;

        public Func<IServiceProvider, ConsumerOptions, Task> ConfigureOptions { get; set; } = null;

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (IsConsuming)
            {
                _logger.LogInformation("{EventConsumer} - Stopping consume events.", typeof(TConsumer).Name);

                if (subscription != null)
                {
                    subscription.Dispose();
                }
                
                IsConsuming = false;

                _logger.LogInformation("{EventConsumer} - Stopped consuming events.", typeof(TConsumer).Name);
            }

            await Task.CompletedTask;
        }

        public void Dispose()
        {
            if (IsConsuming)
            {
                _logger.LogInformation("{EventConsumer} - Stopping consume events.", typeof(TConsumer).Name);

                if (subscription != null)
                {
                    subscription.Dispose();
                }

                IsConsuming = false;

                _logger.LogInformation("{EventConsumer} - Stopped consuming events.", typeof(TConsumer).Name);
            }
            IsEnabled = false;

            if (managementSubscription != null)
            {
                managementSubscription.Dispose();
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (ConfigureOptions != null)
            {
                await ConfigureOptions.Invoke(_serviceProdiver, options);
            }

            if (!options.IsEnabled)
            {
                IsConsuming = false;
                IsEnabled = false;

                return;
            }

            if (!IsConsuming)
            {
                _logger.LogInformation("{EventConsumer} - Starting consume events.", typeof(TConsumer).Name);

                try
                {
                    // Default EasyNetQ Exchange
                    var exchangeName = _conventions.ExchangeNamingConvention(typeof(TMessage));
                    var exchange = await _bus.Advanced.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, true, false, cancellationToken).ConfigureAwait(false);

                    var queueName = $"{_hostEnvironment.ApplicationName}_{typeof(TConsumer).Name}";
                    consumerQueue = await _bus.Advanced.QueueDeclareAsync(
                    queueName, c =>
                    {
                        c.AsDurable(options.Durable);
                        c.AsAutoDelete(options.AutoDelete);

                        if (options.SingleActiveConsumer)
                        {
                            c.WithSingleActiveConsumer();
                        }

                        if (options.MaxPriority.HasValue)
                        {
                            c.WithMaxPriority(options.MaxPriority.Value);
                        }
                    },
                    cancellationToken).ConfigureAwait(false);

                    await _bus.Advanced.BindAsync(exchange, consumerQueue, typeof(TMessage).FullName, cancellationToken).ConfigureAwait(false);

                    var consumerCancellation = _bus.Advanced.Consume<TMessage>(
                    consumerQueue,
                    HandleMessageAsync, c =>
                    {
                        c.WithPrefetchCount(options.PrefetchCount);
                        c.WithPriority(options.Priority);
                        c.WithConsumerTag(_conventions.ConsumerTagConvention());
                    });

                    subscription = new SubscriptionResult(exchange, consumerQueue, consumerCancellation);

                    IsConsuming = (subscription != null);

                    _logger.LogInformation("{EventConsumer} - Started consuming events.", typeof(TConsumer).Name);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "{EventConsumer} - Error on start consuming events.", typeof(TConsumer).Name);
                }
            }

            IsEnabled = true;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            // Register the management subscriber
            managementSubscription = await _bus.PubSub.SubscribeAsync<ManagementEvent>(
                subscriptionId,
                HandleManagementMessageAsync,
                (o) =>
                {
                    o.WithTopic(typeof(TConsumer).FullName);
                    o.WithQueueName($"{_hostEnvironment.ApplicationName}_{typeof(TConsumer).Name}_Management_{Guid.NewGuid()}");
                    o.WithDurable(true);
                    o.WithAutoDelete(true);
                    o.WithPrefetchCount(1);
                    o.AsExclusive(true);
                },
                cancellationToken);

            await StartAsync(cancellationToken);
        }

        private async Task HandleMessageAsync(IMessage<TMessage> message, MessageReceivedInfo messageReceivedInfo, CancellationToken cancellationToken)
        {
            try
            {
                using (var scope = _serviceProdiver.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var consumer = scope.ServiceProvider.GetRequiredService<TConsumer>();

                    await consumer.HandleMessageAsync(message.Body, cancellationToken);
                }
            }
            catch (Exception e)
            {
                var retries = 0;

                if (message.Properties.HeadersPresent && message.Properties.Headers.ContainsKey(RetriesHeaderKey))
                {
                    retries = Convert.ToInt32(message.Properties.Headers[RetriesHeaderKey]);
                }

                if (options.Retries.Any() && retries < options.Retries.Length)
                {
                    var index = Math.Max(0, retries);
                    var delay = TimeSpan.FromMinutes(options.Retries[index]);

                    retries += 1;

                    await RetryAsync(message.Body, delay, messageReceivedInfo.RoutingKey, retries, cancellationToken);

                    _logger.LogWarning(e, "{EventConsumer} - Error consuming an event. Will retry {Atempt} of {MaxAtempts} atempts after {Delay}.",
                        typeof(TConsumer).Name, retries, options.Retries.Length, delay);
                }
                else
                {
                    _logger.LogError(e, "{EventConsumer} - Error consuming the event: \r{Value}",
                        typeof(TConsumer).Name,
                        System.Text.Json.JsonSerializer.Serialize(message.Body, serializeOptions)
                    );

                    throw new Exception($"{typeof(TConsumer).Name} - Error consuming the event", e);
                }
            }
        }

        private async Task RetryAsync(TMessage message, TimeSpan delay, string topic, int retryAtempt, CancellationToken cancellationToken = default)
        {
            // Consumer Exchange
            var consumerExchangeName = $"{_hostEnvironment.ApplicationName}_{typeof(TConsumer).Name}";
            var consumerExchange = await _bus.Advanced.ExchangeDeclareAsync(consumerExchangeName, ExchangeType.Topic, true, false, cancellationToken).ConfigureAwait(false);

            var delayString = delay.ToString(@"dd\_hh\_mm\_ss");
            var futureTopic = $"{topic}_{delayString}";
            var futureQueueName = $"{consumerExchangeName}_{delayString}";
            var futureQueue = await _bus.Advanced.QueueDeclareAsync(
                futureQueueName,
                c =>
                {
                    c.AsDurable(options.Durable);
                    c.AsAutoDelete(true);
                    c.WithMessageTtl(delay);
                    c.WithDeadLetterExchange(consumerExchange);
                    c.WithDeadLetterRoutingKey(topic);
                },
                cancellationToken
            ).ConfigureAwait(false);

            await _bus.Advanced.BindAsync(consumerExchange, consumerQueue, topic, cancellationToken).ConfigureAwait(false);
            await _bus.Advanced.BindAsync(consumerExchange, futureQueue, futureTopic, cancellationToken).ConfigureAwait(false);

            var properties = new MessageProperties
            {
                Priority = 0,
                Headers = new Dictionary<string, object> { { RetriesHeaderKey, retryAtempt } },
                DeliveryMode = 2,
            };
            var advancedMessage = new Message<TMessage>(message, properties);
            await _bus.Advanced.PublishAsync(consumerExchange, futureTopic, true, advancedMessage, cancellationToken).ConfigureAwait(false);
        }

        private async Task HandleManagementMessageAsync(ManagementEvent message, CancellationToken cancellationToken = default)
        {
            switch (message.Action)
            {
                case Enums.ManagementEventActionType.Start:
                    await StartAsync(cancellationToken);
                    break;

                case Enums.ManagementEventActionType.Stop:
                    await StopAsync(cancellationToken);
                    break;

                default:
                    break;
            }
        }
    }
}
