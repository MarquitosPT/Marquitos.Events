namespace Marquitos.Events.RabbitMQ.Consumers
{
    /// <summary>
    /// Event consumer options
    /// </summary>
    public class EventConsumerOptions
    {
        /// <summary>
        /// Topic to listen
        /// </summary>
        public string Topic { get; set; } = "";

        /// <summary>
        /// The queue name 
        /// </summary>
        public string QueueName { get; set; } = "";

        /// <summary>
        /// Array of delay in minutes of retries atempts that the consumer will try to consume the message before raises a failed exception.
        /// </summary>
        public double[] Retries { get; set; } = Array.Empty<double>();

        /// <summary>
        /// Indicates if the messages should be persisted on disk
        /// </summary>
        public bool Durable { get; set; }

        /// <summary>
        /// Indicates id the queue should be removed after consumer disconnect
        /// </summary>
        public bool AutoDelete { get; set; }

        /// <summary>
        /// The amount of messages to retrive from queue each time
        /// </summary>
        public ushort PrefetchCount { get; set; }
    }
}
