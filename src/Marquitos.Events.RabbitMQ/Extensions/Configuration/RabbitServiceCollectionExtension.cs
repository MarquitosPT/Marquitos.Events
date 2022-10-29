using Marquitos.Events;
using Marquitos.Events.RabbitMQ.Consumers;
using Marquitos.Events.RabbitMQ.Services;
using Marquitos.Events.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GDP.Core.Services.RabbitMQ
{
    /// <summary>
    /// Extensions for registering message consumption services via RabbitMQ
    /// </summary>
    public static class RabbitServiceCollectionExtension
    {
        /// <summary>
        /// Register the Connection to RabbitMQ using EasyNetQ
        /// </summary>
        /// <param name="services">This Service Collection</param>
        /// <param name="connectionString">RabbitMQ connection string.</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitConnection(this IServiceCollection services, string connectionString)
        {
            // Register EasyNetQ 
            services.RegisterEasyNetQ(connectionString);

            return services;
        }

        /// <summary>
        /// Register the Event Publishing service
        /// </summary>
        /// <param name="services">This Service Collection</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitEventService(this IServiceCollection services)
        {
            // Event Service
            services.AddScoped<IEventService, RabbitEventService>();

            return services;
        }

        /// <summary>
        /// Register the Event Consumption service
        /// </summary>
        /// <param name="services">This Service Collection</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitConsumerService(this IServiceCollection services)
        {
            // Background Services
            services.AddHostedService<RabbitConsumerService>();

            return services;
        }

        /// <summary>
        /// Register the specified message event consumer
        /// </summary>
        /// <param name="services">This Service Collection</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitEventConsumer<T, TMessage>(this IServiceCollection services) where T : EventConsumer<TMessage> where TMessage : class, IEvent
        {
            services.AddScoped<T>();
            services.AddSingleton<IConsumerService, EventConsumerService<T, TMessage>>();
            services.AddSingleton<IEventConsumerManager<T>, EventConsumerManager<T, TMessage>>();

            return services;
        }
    }
}
