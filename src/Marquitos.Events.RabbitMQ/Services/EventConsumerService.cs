using EasyNetQ;
using Marquitos.Events.RabbitMQ.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marquitos.Events.RabbitMQ.Services
{
    internal class EventConsumerService<TConsumer, TMessage> : IEventConsumerService, IDisposable where TConsumer : EventConsumer<TMessage> where TMessage : class, IEvent
    {
        private readonly IServiceProvider _serviceProdiver;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IBus _bus;
        private readonly ILogger<EventConsumerService<TConsumer, TMessage>> _logger;

        private SubscriptionResult subscription;
        private SubscriptionResult managementSubscription;
        private EventConsumerOptions options;
        private string subscriptionId;

        public EventConsumerService(IServiceProvider serviceProdiver, IHostEnvironment hostEnvironment, IBus bus, ILogger<EventConsumerService<TConsumer, TMessage>> logger)
        {
            _serviceProdiver = serviceProdiver;
            _hostEnvironment = hostEnvironment;
            _bus = bus;
            _logger = logger;

            subscriptionId = _hostEnvironment.ApplicationName;
            options = new EventConsumerOptions
            {
                QueueName = $"{_hostEnvironment.ApplicationName}_{typeof(TMessage).FullName}",
                Durable = true,
                AutoDelete = false,
                PrefetchCount = 1
            };
        }

        public bool IsEnabled { get; protected set; } = false;

        public bool IsConsuming { get; protected set; } = false;

        public Func<IServiceProvider, EventConsumerOptions, Task> ConfigureOptions { get; set; } = null!;

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (IsConsuming)
            {
                _logger.LogInformation("{EventConsumer} - Stopping consume events.", typeof(TConsumer).Name);

                subscription.Dispose();

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

                subscription.Dispose();

                IsConsuming = false;

                _logger.LogInformation("{EventConsumer} - Stopped consuming events.", typeof(TConsumer).Name);
            }
            IsEnabled = false;

            managementSubscription.Dispose();
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
                    subscription = await _bus.PubSub.SubscribeAsync<NotifyEvent<TMessage>>(subscriptionId,
                    HandleMessageAsync,
                    (o) =>
                    {
                        o.WithTopic(NotifyEvent<TMessage>.Key);
                        if (options.QueueName != "")
                        {
                            o.WithQueueName(options.QueueName);
                        }
                        else
                        {
                            o.WithQueueName($"{_hostEnvironment.ApplicationName}_{typeof(TMessage).FullName}");
                        }
                        o.WithDurable(options.Durable);
                        o.WithAutoDelete(options.AutoDelete);
                        o.WithPrefetchCount(options.PrefetchCount);
                        o.WithSingleActiveConsumer(options.SingleActiveConsumer);
                        o.WithPriority(options.Priority);
                    },
                    cancellationToken);

                    IsConsuming = true;

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
            managementSubscription = await _bus.PubSub.SubscribeAsync<ManagementEvent<TConsumer>>(
                subscriptionId,
                HandleManagementMessageAsync,
                (o) =>
                {
                    o.WithTopic(typeof(TConsumer).FullName);
                    o.WithQueueName($"{typeof(TConsumer).FullName}_{Guid.NewGuid()}");
                    o.WithDurable(true);
                    o.WithAutoDelete(true);
                    o.WithPrefetchCount(1);
                    o.WithSingleActiveConsumer(false);
                },
                cancellationToken);

            await StartAsync(cancellationToken);
        }

        private async Task HandleMessageAsync(NotifyEvent<TMessage> message, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var scope = _serviceProdiver.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var consumer = scope.ServiceProvider.GetRequiredService<TConsumer>();

                    await consumer.HandleMessageAsync(message.Value, cancellationToken);
                }
            }
            catch (Exception e)
            {
                if (options.Retries.Any() && message.Retries < options.Retries.Length)
                {
                    var index = Math.Max(0, message.Retries);
                    var delay = TimeSpan.FromMinutes(options.Retries[index]);

                    message.Retries += 1;

                    await _bus.Scheduler.FuturePublishAsync(message, delay, c => c.WithTopic(NotifyEvent<TMessage>.Key), cancellationToken);

                    _logger.LogWarning(e, "{EventConsumer} - Error consuming an event. Will retry {Atempt} of {MaxAtempts} atempts after {Delay}.",
                        typeof(TConsumer).Name, message.Retries, options.Retries.Length, delay);
                }
                else
                {
                    _logger.LogError(e, "{EventConsumer} - Error consuming the event: \r{Value}",
                        typeof(TConsumer).Name, System.Text.Json.JsonSerializer.Serialize(message.Value));

                    throw new Exception($"{typeof(TConsumer).Name} - Error consuming the event", e);
                }
            }
        }

        private async Task HandleManagementMessageAsync(ManagementEvent<TConsumer> message, CancellationToken cancellationToken = default)
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
