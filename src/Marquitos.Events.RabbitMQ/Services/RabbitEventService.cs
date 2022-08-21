using EasyNetQ;
using Marquitos.Events.Services;

namespace Marquitos.Events.RabbitMQ.Services
{
    internal class RabbitEventService : IEventService
    {
        private readonly IBus _rabbitBus;

        public RabbitEventService(IBus rabbitBus)
        {
            _rabbitBus = rabbitBus;
        }

        public async Task NotifyAsync<T>(T eventArgs, CancellationToken cancellationToken = default) where T : class, IEvent
        {
            var notifyEvent = new NotifyEvent<T>(eventArgs);

            await _rabbitBus.PubSub.PublishAsync(notifyEvent, c => c.WithTopic(notifyEvent.Key), cancellationToken);
        }

        public async Task NotifyAsync<T>(T eventArgs, TimeSpan delay, CancellationToken cancellationToken = default) where T : class, IEvent
        {
            var notifyEvent = new NotifyEvent<T>(eventArgs);

            await _rabbitBus.Scheduler.FuturePublishAsync(notifyEvent, delay, c => c.WithTopic(notifyEvent.Key), cancellationToken);
        }
    }
}
