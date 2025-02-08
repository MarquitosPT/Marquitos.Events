using System.Threading;
using System.Threading.Tasks;

namespace Marquitos.Events.RabbitMQ.Services
{
    /// <summary>
    /// Consumer service interface
    /// </summary>
    public interface IConsumerService
    {
        /// <summary>
        /// Gets if the consumer is enabled
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Gets if the consumer is consuming messages
        /// </summary>
        bool IsConsuming { get; }

        /// <summary>
        /// Gets the consumer id
        /// </summary>
        /// <returns></returns>
        string GetConsumerId();

        /// <summary>
        /// Gets the consumer name
        /// </summary>
        /// <returns></returns>
        string GetConsumerName();

        /// <summary>
        /// Gets the consumer message name
        /// </summary>
        /// <returns></returns>
        string GetConsumerMessageName();

        /// <summary>
        /// Gets the consumer queue name
        /// </summary>
        /// <returns></returns>
        string GetConsumerQueueName();

        /// <summary>
        /// Gets the consumer topics
        /// </summary>
        /// <returns></returns>
        string GetConsumerTopics();

        /// <summary>
        /// Gets the consumer host name
        /// </summary>
        /// <returns></returns>
        string GetConsumerHostName();

        /// <summary>
        /// Initializes the consumer
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts the consumer
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops the consumer
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
