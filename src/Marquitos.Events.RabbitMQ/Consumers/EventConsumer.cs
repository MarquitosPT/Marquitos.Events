using Marquitos.Events.Consumers;

namespace Marquitos.Events.RabbitMQ.Consumers
{
    /// <summary>
    /// Abstract event consumer
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class EventConsumer<TMessage>: IEventConsumer where TMessage : class, IEvent
    {
        /// <summary>
        /// Initializes the consumer parameters
        /// </summary>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns true if the consumer is active.</returns>
        /// <remarks>This method is internaly called. It should dot be called directly.</remarks>
        public abstract Task<bool> InitializeAsync(EventConsumerOptions options, CancellationToken cancellationToken = default);

        /// <summary>
        /// Enable the consumer parameter
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <remarks>This method is internaly called. It should dot be called directly.</remarks>
        public abstract Task SetEnabledAsync(bool enabled, CancellationToken cancellationToken = default);

        /// <summary>
        /// Handles a received message from subscribed Queue
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <remarks>This method is internaly called. It should dot be called directly.</remarks>
        public abstract Task HandleMessageAsync(TMessage message, CancellationToken cancellationToken = default);
    }
}
