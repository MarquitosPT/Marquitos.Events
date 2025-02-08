namespace Marquitos.Events.RabbitMQ.Endpoints.Models
{
    /// <summary>
    /// Represents a Consumer definition
    /// </summary>
    public class Consumer
    {
        /// <summary>
        /// Consumer id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Consumer name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Consumer message name
        /// </summary>
        public string MessageName { get; set; }

        /// <summary>
        /// Consumer queue name
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// Consumer topics comma delimited
        /// </summary>
        public string Topics { get; set; }

        /// <summary>
        /// Consumer host name
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Consumer is enabled
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Consumer is consuming messages
        /// </summary>
        public bool IsConsuming { get; set; }
    }
}
