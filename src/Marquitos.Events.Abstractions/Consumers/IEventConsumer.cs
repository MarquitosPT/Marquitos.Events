namespace Marquitos.Events.Consumers
{
    /// <summary>
    /// Base interface for event consumers
    /// </summary>
    public interface IEventConsumer : IBasicConsumer
    {
    }

    /// <summary>
    /// Interface for message event consumers
    /// </summary>
    public interface IEventConsumer<T> : IBasicConsumer<T>, IEventConsumer where T : class, IEvent
    {
    }
}
