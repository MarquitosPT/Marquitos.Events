using EasyNetQ;
using Marquitos.Events.RabbitMQ.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Marquitos.Events.RabbitMQ.Services
{
    internal class EventConsumerService<T, TMessage> : IEventConsumerService where T : EventConsumer<TMessage> where TMessage : class, IEvent
    {
        private readonly IServiceProvider _serviceProdiver;
        private readonly IBus _bus;
        private readonly ILogger<EventConsumerService<T, TMessage>> _logger;
        private SubscriptionResult subscription;
        private EventConsumerOptions consumerOptions = new();

        public EventConsumerService(IServiceProvider serviceProdiver, IBus bus, ILogger<EventConsumerService<T, TMessage>> logger)
        {
            _serviceProdiver = serviceProdiver;
            _bus = bus;
            _logger = logger;

            SubscriptionId = $"{Assembly.GetExecutingAssembly().GetName().Name}";
        }

        public string SubscriptionId { get; set; }

        public bool IsEnabled { get; protected set; }

        public bool IsConsuming { get; protected set; }

        public async Task DisableAsync(CancellationToken cancellationToken = default)
        {
            await StopAsync(cancellationToken);

            using (var scope = _serviceProdiver.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var consumer = scope.ServiceProvider.GetRequiredService<T>();

                await consumer.SetEnabledAsync(false, cancellationToken);
            }

            IsEnabled = false;
        }

        public async Task EnableAsync(CancellationToken cancellationToken = default)
        {
            using (var scope = _serviceProdiver.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var consumer = scope.ServiceProvider.GetRequiredService<T>();

                await consumer.SetEnabledAsync(true, cancellationToken);
            }

            await StartAsync(cancellationToken);
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            using (var scope = _serviceProdiver.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var consumer = scope.ServiceProvider.GetRequiredService<T>();

                IsEnabled = await consumer.InitializeAsync(cancellationToken);

                if (!IsEnabled)
                {
                    await StopAsync(cancellationToken);

                    _logger.LogInformation("{EventConsumer} - Is not enabled.", typeof(T).Name);

                    return;
                }

                if (!IsConsuming)
                {
                    _logger.LogInformation("{EventConsumer} - Starting consume events.", typeof(T).Name);

                    try
                    {
                        consumerOptions = consumer.Options;

                        subscription = await _bus.PubSub.SubscribeAsync<NotifyEvent<TMessage>>(SubscriptionId,
                        HandleMessageAsync,
                        (o) => {
                            if (consumerOptions.Topic != null)
                            {
                                o.WithTopic(consumerOptions.Topic);
                            }
                            if (consumerOptions.QueueName != null)
                            {
                                o.WithQueueName(consumerOptions.QueueName);
                            }
                            o.WithDurable(consumerOptions.Durable);
                            o.WithAutoDelete(consumerOptions.AutoDelete);
                            o.WithPrefetchCount(consumerOptions.PrefetchCount);
                        },
                        cancellationToken);

                        IsConsuming = true;

                        _logger.LogInformation("{EventConsumer} - Started consuming events.", typeof(T).Name);
                    }
                    catch (Exception e) 
                    {
                        _logger.LogError(e, "{EventConsumer} - Error on start consuming events.", typeof(T).Name);
                    }
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (IsConsuming)
            {
                _logger.LogInformation("{EventConsumer} - Stopping consume events.", typeof(T).Name);

                subscription.Dispose();

                IsConsuming = false;

                _logger.LogInformation("{EventConsumer} - Stopped consuming events.", typeof(T).Name);
            }

            await Task.CompletedTask;
        }

        private async Task HandleMessageAsync(NotifyEvent<TMessage> message, CancellationToken cancellationToken = default)
        {
            using (var scope = _serviceProdiver.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var consumer = scope.ServiceProvider.GetRequiredService<T>();
                try
                {
                    await consumer.HandleMessageAsync(message.Value, cancellationToken);
                }
                catch (Exception e)
                {
                    if (consumerOptions.Retries.Any() && (message.Retries < consumerOptions.Retries.Count()))
                    {
                        var index = Math.Max(0, message.Retries);
                        var delay = TimeSpan.FromMinutes(consumerOptions.Retries[index]);

                        message.Retries += 1;

                        var rabbitBus = scope.ServiceProvider.GetRequiredService<IBus>();
                        await rabbitBus.Scheduler.FuturePublishAsync(message, delay, c => c.WithTopic(message.Key), cancellationToken);

                        _logger.LogWarning(e, "{EventConsumer} - Error consuming an event. Will retry {Atempt} of {MaxAtempts} atempts after {Delay}.", 
                            typeof(T).Name, message.Retries, consumerOptions.Retries.Count(), delay);
                    }
                    else
                    {
                        _logger.LogError(e, "{EventConsumer} - Error consuming the event: {Value}",
                            typeof(T).Name, System.Text.Json.JsonSerializer.Serialize(message.Value));

                        throw new Exception($"{typeof(T).Name} - Error consuming the event", e);
                    }
                }
                
            }  
        }

    }
}
