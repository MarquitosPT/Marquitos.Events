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
    internal class BasicConsumerService<TConsumer, TMessage> : IBasicConsumerService, IDisposable where TConsumer : BasicConsumer<TMessage> where TMessage : class
    {
        private readonly IServiceProvider _serviceProdiver;
        private readonly IHostEnvironment _hostEnvironment;

        private readonly IBus _bus;
        private readonly IConventions _conventions;
        private readonly ILogger<BasicConsumerService<TConsumer, TMessage>> _logger;

        private IDisposable subscription;
        private IDisposable managementSubscription;
        private ConsumerOptions options;
        private string subscriptionId;
        private string consumerId;
        private string consumerName;
        private string consumerMessageName;
        private string consumerQueueName;
        private string managementQueueName;
        private string managementTopic;
        private string queueName;
        private string exchangeName;
        private ICollection<string> topics = new List<string>();
        private JsonSerializerOptions serializeOptions;

        private Queue consumerQueue;

        private const string RetriesHeaderKey = "x-retries";

        public BasicConsumerService(IServiceProvider serviceProdiver, IHostEnvironment hostEnvironment, IBus bus, IConventions conventions, ILogger<BasicConsumerService<TConsumer, TMessage>> logger)
        {
            _serviceProdiver = serviceProdiver;
            _hostEnvironment = hostEnvironment;
            _bus = bus;
            _conventions = conventions;
            _logger = logger;

            subscriptionId = _hostEnvironment.ApplicationName;
            consumerId = Guid.NewGuid().ToString();
            consumerName = typeof(TConsumer).Name;
            consumerMessageName = typeof(TMessage).Name;
            consumerQueueName = $"{_hostEnvironment.ApplicationName}_{typeof(TConsumer).Name}";

            managementQueueName = $"{_hostEnvironment.ApplicationName}_{consumerName}_Management_{consumerId}";
            managementTopic = typeof(TConsumer).FullName;

            exchangeName = _conventions.ExchangeNamingConvention(typeof(TMessage));
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

            _logger.BeginScope("{BasicConsumer}", consumerName);
        }

        public bool IsEnabled { get; protected set; } = false;

        public bool IsConsuming { get; protected set; } = false;

        public void SetQueueName(string name)
        {
            queueName = name;
        }

        public void AddTopic(string topic)
        {
            if (!string.IsNullOrWhiteSpace(topic))
            {
                topics.Add(topic);
            }
        }

        public void AddTopics(string[] topics)
        {
            if (topics != null)
            {
                foreach (string topic in topics) { this.topics.Add(topic); }
            }
        }

        public Func<IServiceProvider, ConsumerOptions, Task> ConfigureOptions { get; set; } = null;

        public string GetConsumerId()
        {
            return consumerId;
        }

        public string GetConsumerName()
        {
            return consumerName;
        }

        public string GetConsumerMessageName()
        {
            return consumerMessageName;
        }

        public string GetConsumerQueueName()
        {
            return consumerQueueName;
        }

        public string GetConsumerTopics()
        {
            return string.Join(",", topics);
        }

        public string GetConsumerHostName()
        {
            return Environment.MachineName;
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (IsConsuming)
            {
                _logger.LogInformation("{BasicConsumer} - Stopping consume events.", consumerName);

                if (subscription != null)
                {
                    subscription.Dispose();
                }
                
                IsConsuming = false;

                _logger.LogInformation("{BasicConsumer} - Stopped consuming events.", consumerName);
            }

            await Task.CompletedTask;
        }

        public void Dispose()
        {
            if (IsConsuming)
            {
                _logger.LogInformation("{BasicConsumer} - Stopping consume events.", consumerName);

                if (subscription != null)
                {
                    subscription.Dispose();
                }

                IsConsuming = false;

                _logger.LogInformation("{BasicConsumer} - Stopped consuming events.", consumerName);
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
                _logger.LogInformation("{BasicConsumer} - Starting consume events.", consumerName);

                try
                {
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

                    if (topics.Any())
                    {
                        // Default EasyNetQ Exchange
                        var exchange = await _bus.Advanced.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, true, false, cancellationToken).ConfigureAwait(false);

                        foreach (var topic in topics)
                        {
                            await _bus.Advanced.BindAsync(exchange, consumerQueue, topic, cancellationToken).ConfigureAwait(false);
                        }
                    }

                    var consumerCancellation = _bus.Advanced.Consume<TMessage>(
                    consumerQueue,
                    HandleMessageAsync, c =>
                    {
                        c.WithPrefetchCount(options.PrefetchCount);
                        c.WithPriority(options.Priority);
                        c.WithConsumerTag(_conventions.ConsumerTagConvention());
                    });

                    subscription = consumerCancellation;

                    IsConsuming = (subscription != null);

                    _logger.LogInformation("{BasicConsumer} - Started consuming events.", consumerName);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "{BasicConsumer} - Error on start consuming events.", consumerName);
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
                    o.WithTopic(managementTopic);
                    o.WithQueueName(managementQueueName);
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

                    _logger.LogWarning(e, "{BasicConsumer} - Error consuming an event. Will retry {Atempt} of {MaxAtempts} atempts after {Delay}.",
                        consumerName, retries, options.Retries.Length, delay);
                }
                else
                {
                    _logger.LogError(e, "{BasicConsumer} - Error consuming the event: \r{Value}",
                        consumerName,
                        System.Text.Json.JsonSerializer.Serialize(message.Body, serializeOptions)
                    );

                    throw new Exception($"{consumerName} - Error consuming the event", e);
                }
            }
        }

        private async Task RetryAsync(TMessage message, TimeSpan delay, string topic, int retryAtempt, CancellationToken cancellationToken = default)
        {
            // Consumer Exchange
            var consumerExchangeName = $"{_hostEnvironment.ApplicationName}_{consumerName}";
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
