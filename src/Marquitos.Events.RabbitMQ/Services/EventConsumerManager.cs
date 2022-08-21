using Marquitos.Events.RabbitMQ.Consumers;

namespace Marquitos.Events.RabbitMQ.Services
{
    internal class EventConsumerManager<T, TMessage>: IEventConsumerManager<T, TMessage> where T : EventConsumer<TMessage> where TMessage : class, IEvent
    {
        private readonly EventConsumerService<T, TMessage> _service;

        public EventConsumerManager(IEnumerable<IConsumerService> services)
        {
            _service = (EventConsumerService<T, TMessage>)services.First(e => e is EventConsumerService<T, TMessage>);
        }

        public bool IsEnabled => _service.IsEnabled;
        public bool IsConsuming => _service.IsConsuming;

        public async Task EnableAsync(CancellationToken cancellationToken = default)
        {
            await _service.EnableAsync(cancellationToken);
        }

        public async Task DisableAsync(CancellationToken cancellationToken = default)
        {
            await _service.DisableAsync(cancellationToken);
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await _service.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _service.StopAsync(cancellationToken);
        }
    }
}
