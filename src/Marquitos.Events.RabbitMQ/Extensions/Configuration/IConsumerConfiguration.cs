using Marquitos.Events.Consumers;
using Marquitos.Events.RabbitMQ.Consumers;
using System;
using System.Threading.Tasks;

namespace Marquitos.Events.RabbitMQ.Extensions.Configuration
{
    /// <summary>
    /// Interface for generic Consumer configuration
    /// </summary>
    /// <typeparam name="TConsumer"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public interface IConsumerConfiguration<TConsumer, TMessage> where TConsumer : class, IBasicConsumer<TMessage> where TMessage : class
    {
        /// <summary>
        /// Configure task options
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        Task ConfigureAsync(IServiceProvider serviceProvider, ConsumerOptions options);
    }
}
