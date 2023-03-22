﻿using Marquitos.Events.Consumers;
using System.Threading;
using System.Threading.Tasks;

namespace Marquitos.Events.RabbitMQ.Consumers
{
    /// <summary>
    /// Abstract basic consumer
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class BasicConsumer<TMessage> : IBasicConsumer<TMessage> where TMessage : class
    {
        /// <summary>
        /// Handles a received message from subscribed Queue
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <remarks>This method is internaly called. It should dot be called directly.</remarks>
        public abstract Task HandleMessageAsync(TMessage message, CancellationToken cancellationToken = default);
    }
}
