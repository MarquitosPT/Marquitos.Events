using EasyNetQ;
using EasyNetQ.DI;
using Marquitos.Events.RabbitMQ.Consumers;
using Marquitos.Events.RabbitMQ.Services;
using Marquitos.Events.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Marquitos.Events.RabbitMQ.Extensions.Configuration
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
        /// <param name="registerServices">Action to configure EasyNetQ parameters</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQConnection(this IServiceCollection services, string connectionString, Action<IServiceRegister> registerServices)
        {
            // Register EasyNetQ 
            services.RegisterEasyNetQ(connectionString, registerServices);

            return services;
        }

#if NET6_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER

        /// <summary>
        /// Register the Connection to RabbitMQ using EasyNetQ with Newtosoft serialization
        /// </summary>
        /// <param name="services">This Service Collection</param>
        /// <param name="connectionString">RabbitMQ connection string.</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQConnectionWithNewtonsoftJson(this IServiceCollection services, string connectionString)
        {
            // Register EasyNetQ 
            services.RegisterEasyNetQ(connectionString, o =>
            {
                o.EnableNewtonsoftJson();
            });

            return services;
        }

        /// <summary>
        /// Register the Connection to RabbitMQ using EasyNetQ with Microsoft serialization
        /// </summary>
        /// <param name="services">This Service Collection</param>
        /// <param name="connectionString">RabbitMQ connection string.</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQConnectionWithSystemTextJson(this IServiceCollection services, string connectionString)
        {
            // Register EasyNetQ 
            services.RegisterEasyNetQ(connectionString, o =>
            {
                o.EnableSystemTextJson();
            });

            return services;
        }

#endif

        /// <summary>
        /// Register the Event Publishing service
        /// </summary>
        /// <param name="services">This Service Collection</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQEventService(this IServiceCollection services)
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
        public static IServiceCollection AddRabbitMQConsumerService(this IServiceCollection services)
        {
            // Background Services
            services.AddHostedService<RabbitConsumerService>();

            return services;
        }

        /// <summary>
        /// Register the specified message event consumer
        /// </summary>
        /// <param name="services">This Service Collection</param>
        /// <param name="configureOptions">Function to asynchronous configure the consumer options.</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQEventConsumer<TConsumer, TMessage>(this IServiceCollection services, Action<IServiceProvider, EventConsumerOptions> configureOptions) where TConsumer : EventConsumer<TMessage> where TMessage : class, IEvent
        {
            services.AddScoped<TConsumer>();
            services.AddSingleton<IConsumerService, EventConsumerService<TConsumer, TMessage>>((serviceProvider) =>
            {
#if NETCOREAPP2_1
                var hostEnvironment = serviceProvider.GetRequiredService<IHostingEnvironment>();
#else
                var hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
#endif
                var bus = serviceProvider.GetRequiredService<IBus>();
                var conventions = serviceProvider.GetRequiredService<IConventions>();
                var logger = serviceProvider.GetRequiredService<ILogger<EventConsumerService<TConsumer, TMessage>>>();

                var result = new EventConsumerService<TConsumer, TMessage>(serviceProvider, hostEnvironment, bus, conventions,logger)
                {
                    ConfigureOptions = async (sp, st) =>
                    {
                        if (configureOptions != null)
                        {
                            configureOptions.Invoke(sp, st);

                            await Task.CompletedTask;
                        }
                    }
                };

                return result;
            });
            services.AddSingleton<IEventConsumerManager<TConsumer>, EventConsumerManager<TConsumer, TMessage>>();

            return services;
        }

        /// <summary>
        /// Register the specified message event consumer
        /// </summary>
        /// <param name="services">This Service Collection</param>
        /// <param name="configureOptions">Function to asynchronous configure the consumer options.</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQEventConsumer<TConsumer, TMessage>(this IServiceCollection services, Func<IServiceProvider, EventConsumerOptions, Task> configureOptions) where TConsumer : EventConsumer<TMessage> where TMessage : class, IEvent
        {
            services.AddScoped<TConsumer>();
            services.AddSingleton<IConsumerService, EventConsumerService<TConsumer, TMessage>>((serviceProvider) =>
            {
#if NETCOREAPP2_1
                var hostEnvironment = serviceProvider.GetRequiredService<IHostingEnvironment>();
#else
                var hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
#endif
                var bus = serviceProvider.GetRequiredService<IBus>();
                var conventions = serviceProvider.GetRequiredService<IConventions>();
                var logger = serviceProvider.GetRequiredService<ILogger<EventConsumerService<TConsumer, TMessage>>>();

                var result = new EventConsumerService<TConsumer, TMessage>(serviceProvider, hostEnvironment, bus, conventions, logger)
                {
                    ConfigureOptions = async (sp, st) =>
                    {
                        if (configureOptions != null)
                        {
                            await configureOptions.Invoke(sp, st);
                        }
                    }
                };

                return result;
            });
            services.AddSingleton<IEventConsumerManager<TConsumer>, EventConsumerManager<TConsumer, TMessage>>();

            return services;
        }

        /// <summary>
        /// Register the specified message event consumer
        /// </summary>
        /// <param name="services">This Service Collection</param>
        /// <param name="eventCosumerConfiguration">A class that implements the <see cref="IEventConsumerConfiguration{TConsumer, TMessage}"/> to asynchronous configure the evet consumer options.</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQEventConsumer<TConsumer, TMessage>(this IServiceCollection services, IEventConsumerConfiguration<TConsumer, TMessage> eventCosumerConfiguration) where TConsumer : EventConsumer<TMessage> where TMessage : class, IEvent
        {
            services.AddScoped<TConsumer>();
            services.AddSingleton<IConsumerService, EventConsumerService<TConsumer, TMessage>>((serviceProvider) =>
            {
#if NETCOREAPP2_1
                var hostEnvironment = serviceProvider.GetRequiredService<IHostingEnvironment>();
#else
                var hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
#endif
                var bus = serviceProvider.GetRequiredService<IBus>();
                var conventions = serviceProvider.GetRequiredService<IConventions>();
                var logger = serviceProvider.GetRequiredService<ILogger<EventConsumerService<TConsumer, TMessage>>>();

                var result = new EventConsumerService<TConsumer, TMessage>(serviceProvider, hostEnvironment, bus, conventions, logger)
                {
                    ConfigureOptions = async (sp, st) =>
                    {
                        if (eventCosumerConfiguration != null)
                        {
                            await eventCosumerConfiguration.ConfigureAsync(sp, st);
                        }
                    }
                };

                return result;
            });
            services.AddSingleton<IEventConsumerManager<TConsumer>, EventConsumerManager<TConsumer, TMessage>>();

            return services;
        }
    }
}
