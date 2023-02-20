using EasyNetQ;
using Marquitos.Events.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

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
            await _rabbitBus.PubSub.PublishAsync(eventArgs, c => c.WithTopic(typeof(T).FullName), cancellationToken);
        }

        public async Task NotifyAsync<T>(T eventArgs, TimeSpan delay, CancellationToken cancellationToken = default) where T : class, IEvent
        {
            await _rabbitBus.Scheduler.FuturePublishAsync(eventArgs, delay, c => c.WithTopic(typeof(T).FullName), cancellationToken);
        }
    }
}
