using System;

namespace Marquitos.Events.RabbitMQ.Consumers
{
    /// <summary>
    /// Consumer options
    /// </summary>
    public class ConsumerOptions
    {
        /// <summary>
        /// Array of delay in minutes of retries atempts that the consumer will try to consume the message before raises a failed exception.
        /// </summary>
        public double[] Retries { get; set; } = Array.Empty<double>();

        /// <summary>
        /// Indicates if the messages should be persisted on disk
        /// </summary>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// Indicates id the queue should be removed after consumer disconnect
        /// </summary>
        public bool AutoDelete { get; set; } = false;

        /// <summary>
        /// Configure the queue as single active consumer.
        /// </summary>
        public bool SingleActiveConsumer { get; set; } = false;

        /// <summary>
        /// The amount of messages to retrive from queue each time
        /// </summary>
        public ushort PrefetchCount { get; set; } = 1;

        /// <summary>
        /// Configures the consumer's priority
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Configures the Queue max priority
        /// </summary>
        public int? MaxPriority { get; set; } = null;

        /// <summary>
        /// Indicates if the consumer is active and should consume messages.
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }
}
