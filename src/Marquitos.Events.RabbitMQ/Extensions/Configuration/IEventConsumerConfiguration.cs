using Marquitos.Events.RabbitMQ.Consumers;
using System;
using System.Threading.Tasks;

namespace Marquitos.Events.RabbitMQ.Extensions.Configuration
{
    /// <summary>
    /// Interface for generic Event Consumer configuration
    /// </summary>
    /// <typeparam name="TConsumer"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public interface IEventConsumerConfiguration<TConsumer, TMessage> where TConsumer : EventConsumer<TMessage> where TMessage : class, IEvent
    {
        /// <summary>
        /// Configure task options
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        Task ConfigureAsync(IServiceProvider serviceProvider, EventConsumerOptions options);
    }
}
