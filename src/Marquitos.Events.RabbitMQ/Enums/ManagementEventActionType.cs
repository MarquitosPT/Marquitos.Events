namespace Marquitos.Events.RabbitMQ.Enums
{
    /// <summary>
    /// ManagementEvent Actions
    /// </summary>
    public enum ManagementEventActionType
    {
        /// <summary>
        /// Enable the consumer and start consuming
        /// </summary>
        Enable,

        /// <summary>
        /// Start consuming if enabled
        /// </summary>
        Start,

        /// <summary>
        /// Stop consuming
        /// </summary>
        Stop,

        /// <summary>
        /// Stop and disable the consumer
        /// </summary>
        Disable
    }
}
