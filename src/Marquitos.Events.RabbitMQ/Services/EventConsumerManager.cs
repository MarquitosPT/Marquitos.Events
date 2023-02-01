using EasyNetQ;
using Marquitos.Events.RabbitMQ.Consumers;

namespace Marquitos.Events.RabbitMQ.Services
{
    internal class EventConsumerManager<T, TMessage> : IEventConsumerManager<T> where T : EventConsumer<TMessage> where TMessage : class, IEvent
    {
        private readonly IConsumerService _service;
        private readonly IBus _Bus;

        public EventConsumerManager(IEnumerable<IConsumerService> services, IBus bus)
        {
            _service = services.First(e => e is EventConsumerService<T, TMessage>);
            _Bus = bus;
        }

        public bool IsEnabled => _service.IsEnabled;
        public bool IsConsuming => _service.IsConsuming;

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            var notifyEvent = new ManagementEvent<T>
            {
                Action = Enums.ManagementEventActionType.Start
            };

            await _Bus.PubSub.PublishAsync(notifyEvent, c => c.WithTopic(ManagementEvent<T>.Key), cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            var notifyEvent = new ManagementEvent<T>
            {
                Action = Enums.ManagementEventActionType.Stop
            };

            await _Bus.PubSub.PublishAsync(notifyEvent, c => c.WithTopic(ManagementEvent<T>.Key), cancellationToken);
        }
    }
}
