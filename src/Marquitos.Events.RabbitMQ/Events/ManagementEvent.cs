using Marquitos.Events.Consumers;
using Marquitos.Events.RabbitMQ.Enums;

namespace Marquitos.Events
{
    /// <summary>
    /// Management event fired by management service
    /// </summary>
    internal class ManagementEvent<TConsumer> where TConsumer : class, IEventConsumer
    {
        public ManagementEventActionType Action { get; set; }

        /// <summary>
        /// Gets the unique identifier key for this event
        /// </summary>
        public static string Key => $"{typeof(TConsumer).FullName}";

    }
}
