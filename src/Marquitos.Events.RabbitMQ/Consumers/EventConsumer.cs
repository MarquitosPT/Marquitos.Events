namespace Marquitos.Events.RabbitMQ.Consumers
{
    /// <summary>
    /// Abstract event consumer
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class EventConsumer<TMessage> where TMessage : class, IEvent
    {
        /// <summary>
        /// Creates a new instance
        /// </summary>
        public EventConsumer()
        {
            Options = new EventConsumerOptions
            {
                Topic = $"{typeof(TMessage).FullName}",
                QueueName = "",
                Durable = true,
                AutoDelete = false,
                PrefetchCount = 1
            };
        }

        /// <summary>
        /// Consumer options
        /// </summary>
        public EventConsumerOptions Options { get; set; }

        /// <summary>
        /// Initializes the consumer parameters
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns true if the consumer is active.</returns>
        /// <remarks>This method is internaly called. It should dot be called directly.</remarks>
        public abstract Task<bool> InitializeAsync(CancellationToken cancellationToken = default);

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
