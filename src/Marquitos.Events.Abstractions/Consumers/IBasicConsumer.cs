using System.Threading;
using System.Threading.Tasks;

namespace Marquitos.Events.Consumers
{
    /// <summary>
    /// Base interface for basic consumers
    /// </summary>
    public interface IBasicConsumer
    {
    }

    /// <summary>
    /// Interface for message basic consumers
    /// </summary>
    public interface IBasicConsumer<T> : IBasicConsumer where T : class
    {
        /// <summary>
        /// Handles a received message from subscribed Queue
        /// </summary>
        /// <param name="message">message data</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <remarks>This method is internaly called. It should dot be called directly.</remarks>
        Task HandleMessageAsync(T message, CancellationToken cancellationToken = default);
    }
}
