using Marquitos.Events.Consumers;
using System.Threading;
using System.Threading.Tasks;

namespace Marquitos.Events.RabbitMQ.Services
{
    /// <summary>
    /// Interface for managing consumer services
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConsumerManager<T> where T : class, IBasicConsumer
    {
        /// <summary>
        /// Retrives information if the consumer is Enabled
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Retrives information if the consumer is currently consuming messages
        /// </summary>
        bool IsConsuming { get; }

        /// <summary>
        /// Start consuming messages
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <remarks>The service will not start consuming messages if the service isn't Enabled.</remarks>
        /// <returns></returns>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stop consuming messages
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <remarks>The service will only stop consuming messages until next restart.</remarks>
        /// <returns></returns>
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
