using EasyNetQ;
using Marquitos.Events.RabbitMQ.Consumers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Marquitos.Events.RabbitMQ.Services
{
    internal class EventConsumerManager<TConsumer, TMessage> : IEventConsumerManager<TConsumer> where TConsumer : EventConsumer<TMessage> where TMessage : class, IEvent
    {
        private readonly IConsumerService _service;
        private readonly IBus _Bus;

        public EventConsumerManager(IEnumerable<IConsumerService> services, IBus bus)
        {
            _service = services.First(e => e is EventConsumerService<TConsumer, TMessage>);
            _Bus = bus;
        }

        public bool IsEnabled => _service.IsEnabled;
        public bool IsConsuming => _service.IsConsuming;

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            var notifyEvent = new ManagementEvent
            {
                Action = Enums.ManagementEventActionType.Start
            };

            await _Bus.PubSub.PublishAsync(notifyEvent, c => c.WithTopic(typeof(TConsumer).FullName), cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            var notifyEvent = new ManagementEvent
            {
                Action = Enums.ManagementEventActionType.Stop
            };

            await _Bus.PubSub.PublishAsync(notifyEvent, c => c.WithTopic(typeof(TConsumer).FullName), cancellationToken);
        }
    }
}
