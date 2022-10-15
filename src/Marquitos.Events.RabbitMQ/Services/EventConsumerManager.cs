using EasyNetQ;
using Marquitos.Events.RabbitMQ.Consumers;
using Microsoft.Extensions.DependencyInjection;

namespace Marquitos.Events.RabbitMQ.Services
{
    internal class EventConsumerManager<T, TMessage> : IEventConsumerManager<T> where T : EventConsumer<TMessage> where TMessage : class, IEvent
    {
        private readonly IConsumerService _service;
        private readonly IServiceProvider _serviceProdiver;

        public EventConsumerManager(IEnumerable<IConsumerService> services, IServiceProvider serviceProdiver)
        {
            _service = services.First(e => e is EventConsumerService<T, TMessage>);
            _serviceProdiver = serviceProdiver;
        }

        public bool IsEnabled => _service.IsEnabled;
        public bool IsConsuming => _service.IsConsuming;

        public async Task EnableAsync(CancellationToken cancellationToken = default)
        {
            using (var scope = _serviceProdiver.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var rabbitBus = scope.ServiceProvider.GetRequiredService<IBus>();
                var notifyEvent = new ManagementEvent<T>
                {
                    Action = Enums.ManagementEventActionType.Enable
                };

                await rabbitBus.PubSub.PublishAsync(notifyEvent, c => c.WithTopic(ManagementEvent<T>.Key), cancellationToken);
            }
        }

        public async Task DisableAsync(CancellationToken cancellationToken = default)
        {
            using (var scope = _serviceProdiver.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var rabbitBus = scope.ServiceProvider.GetRequiredService<IBus>();
                var notifyEvent = new ManagementEvent<T>
                {
                    Action = Enums.ManagementEventActionType.Disable
                };

                await rabbitBus.PubSub.PublishAsync(notifyEvent, c => c.WithTopic(ManagementEvent<T>.Key), cancellationToken);
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            using (var scope = _serviceProdiver.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var rabbitBus = scope.ServiceProvider.GetRequiredService<IBus>();
                var notifyEvent = new ManagementEvent<T>
                {
                    Action = Enums.ManagementEventActionType.Start
                };

                await rabbitBus.PubSub.PublishAsync(notifyEvent, c => c.WithTopic(ManagementEvent<T>.Key), cancellationToken);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            using (var scope = _serviceProdiver.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var rabbitBus = scope.ServiceProvider.GetRequiredService<IBus>();
                var notifyEvent = new ManagementEvent<T>
                {
                    Action = Enums.ManagementEventActionType.Stop
                };

                await rabbitBus.PubSub.PublishAsync(notifyEvent, c => c.WithTopic(ManagementEvent<T>.Key), cancellationToken);
            }
        }
    }
}
