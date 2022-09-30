namespace Marquitos.Events
{
    /// <summary>
    /// Notify Event for a specific event T
    /// </summary>
    /// <typeparam name="T">A class that implements the <see cref="IEvent"/> interface.</typeparam>
    public class NotifyEvent<T> where T : class, IEvent
    {
        /// <summary>
        /// Creates a new instance
        /// </summary>
        public NotifyEvent()
        {
            Value = null!;
        }

        public NotifyEvent(T value)
        {
            Value = value;
        }

        public NotifyEvent(T value, string origin)
        {
            Value = value;
            Origin = origin;
        }

        /// <summary>
        /// Event content
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Gets the unique identifier key for this event
        /// </summary>
        public string Key => $"{ typeof(T).FullName }";

        /// <summary>
        /// Origin aplication that fired the event
        /// </summary>
        public string Origin { get; set; } = "";

        /// <summary>
        /// Number of retries that already been made to consume the event
        /// </summary>
        public int Retries { get; set; } = 0;
    }
}
