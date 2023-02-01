using System.Threading.Tasks;
using System.Threading;

namespace Marquitos.Events.Consumers
{
    /// <summary>
    /// Base interface for event consumers
    /// </summary>
    public interface IEventConsumer
    {
    }

    /// <summary>
    /// Interface for message event consumers
    /// </summary>
    public interface IEventConsumer<T> : IEventConsumer where T : class, IEvent
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
