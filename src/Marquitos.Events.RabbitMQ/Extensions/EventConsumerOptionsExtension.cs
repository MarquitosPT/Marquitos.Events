using Marquitos.Events.RabbitMQ.Consumers;
using System;

namespace Marquitos.Events.RabbitMQ.Extensions
{
    /// <summary>
    /// EventConsumerOptions Extension
    /// </summary>
    public static class EventConsumerOptionsExtension
    {
        /// <summary>
        /// Update current options from other source options
        /// </summary>
        /// <param name="options">This options</param>
        /// <param name="sourceOptions">Source options</param>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if sourceOptions is null.</exception>
        public static void UpdateFrom(this EventConsumerOptions options, EventConsumerOptions sourceOptions)
        {
            if (sourceOptions == null)
            {
                throw new ArgumentNullException(nameof(sourceOptions));
            }

            options.PrefetchCount = sourceOptions.PrefetchCount;
            options.Priority = sourceOptions.Priority;
            options.MaxPriority= sourceOptions.MaxPriority;
            options.Retries = sourceOptions.Retries;
            options.SingleActiveConsumer = sourceOptions.SingleActiveConsumer;
            options.AutoDelete = sourceOptions.AutoDelete;
            options.Durable = sourceOptions.Durable;
        }
    }
}
