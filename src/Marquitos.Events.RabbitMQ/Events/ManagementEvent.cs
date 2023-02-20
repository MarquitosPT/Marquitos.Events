using Marquitos.Events.RabbitMQ.Enums;

namespace Marquitos.Events
{
    /// <summary>
    /// Management event fired by management service
    /// </summary>
    internal class ManagementEvent
    {
        public ManagementEventActionType Action { get; set; }
    }
}
