namespace Marquitos.Events.Services
{
    /// <summary>
    /// Interface to notify events
    /// </summary>
    public interface IEventService
    {
        /// <summary>
        /// Notify event
        /// </summary>
        /// <param name="eventArgs">event data</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task NotifyAsync<T>(T eventArgs, CancellationToken cancellationToken = default) where T : class, IEvent;

        /// <summary>
        /// Notify event to be fired at some time in the future
        /// </summary>
        /// <param name="eventArgs">event data</param>
        /// <param name="delay">amount of time to wait before the event is fired.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task NotifyAsync<T>(T eventArgs, TimeSpan delay, CancellationToken cancellationToken = default) where T : class, IEvent;
    }
}
