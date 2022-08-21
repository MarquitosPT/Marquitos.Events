using Marquitos.Events;
using Marquitos.Events.RabbitMQ.Consumers;
using Marquitos.Events.RabbitMQ.Services;
using Marquitos.Events.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GDP.Core.Services.RabbitMQ
{
    /// <summary>
    /// Extensões para registo de serviços de consumo de mensagens via RabbitMQ
    /// </summary>
    public static class RabbitServiceCollectionExtension
    {
        /// <summary>
        /// Regista o serviço de Publicação de Eventos
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitEventService(this IServiceCollection services)
        {
            // Event Service
            services.AddScoped<IEventService, RabbitEventService>();

            return services;
        }

        /// <summary>
        /// Regista o serviço de Consumo de Eventos
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitConsumerService(this IServiceCollection services)
        {
            // Background Services
            services.AddHostedService<RabbitConsumerService>();

            return services;
        }

        /// <summary>
        /// Regista o consumidor de eventos de mensagens via RabbitMQ
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddEventConsumer<T, TMessage>(this IServiceCollection services) where T : EventConsumer<TMessage> where TMessage : class, IEvent
        {
            services.AddScoped<T>();
            services.AddSingleton<IConsumerService, EventConsumerService<T, TMessage>>();
            services.AddSingleton<IEventConsumerManager<T, TMessage>, EventConsumerManager<T, TMessage>>();

            return services;
        }
    }
}
