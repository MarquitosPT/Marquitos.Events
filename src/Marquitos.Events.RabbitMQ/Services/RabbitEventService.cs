using EasyNetQ;
using Marquitos.Events.Services;
using Microsoft.Extensions.Hosting;

namespace Marquitos.Events.RabbitMQ.Services
{
    internal class RabbitEventService : IEventService
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IBus _rabbitBus;

        public RabbitEventService(IHostEnvironment hostEnvironment, IBus rabbitBus)
        {
            _hostEnvironment = hostEnvironment;
            _rabbitBus = rabbitBus;
        }

        public async Task NotifyAsync<T>(T eventArgs, CancellationToken cancellationToken = default) where T : class, IEvent
        {
            var notifyEvent = new NotifyEvent<T>(eventArgs, _hostEnvironment.ApplicationName);

            await _rabbitBus.PubSub.PublishAsync(notifyEvent, c => c.WithTopic(NotifyEvent<T>.Key), cancellationToken);
        }

        public async Task NotifyAsync<T>(T eventArgs, TimeSpan delay, CancellationToken cancellationToken = default) where T : class, IEvent
        {
            var notifyEvent = new NotifyEvent<T>(eventArgs, _hostEnvironment.ApplicationName);

            await _rabbitBus.Scheduler.FuturePublishAsync(notifyEvent, delay, c => c.WithTopic(NotifyEvent<T>.Key), cancellationToken);
        }
    }
}
