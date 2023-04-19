using EasyNetQ;
using EasyNetQ.DI;
using Marquitos.Events.RabbitMQ.Builders;
using Marquitos.Events.RabbitMQ.Consumers;
using Marquitos.Events.RabbitMQ.Converters;
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
                var serializeOptions = new System.Text.Json.JsonSerializerOptions();
                serializeOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
#if NET6_0_OR_GREATER
                serializeOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                serializeOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                serializeOptions.Converters.Add(new DateOnlyJsonConverter());
                serializeOptions.Converters.Add(new TimeOnlyJsonConverter());
#endif
                o.EnableSystemTextJson(serializeOptions);
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
        public static IServiceCollection AddRabbitMQEventConsumer<TConsumer, TMessage>(this IServiceCollection services, Action<IServiceProvider, ConsumerOptions> configureOptions) where TConsumer : EventConsumer<TMessage> where TMessage : class, IEvent
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
                            configureOptions.Invoke(sp, st);

                            await Task.CompletedTask;
                        }
                    }
                };

                return result;
            });
            services.AddSingleton<IConsumerManager<TConsumer>, EventConsumerManager<TConsumer, TMessage>>();

            return services;
        }

        /// <summary>
        /// Register the specified message event consumer
        /// </summary>
        /// <param name="services">This Service Collection</param>
        /// <param name="configureOptions">Function to asynchronous configure the consumer options.</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQEventConsumer<TConsumer, TMessage>(this IServiceCollection services, Func<IServiceProvider, ConsumerOptions, Task> configureOptions) where TConsumer : EventConsumer<TMessage> where TMessage : class, IEvent
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
            services.AddSingleton<IConsumerManager<TConsumer>, EventConsumerManager<TConsumer, TMessage>>();

            return services;
        }

        /// <summary>
        /// Register the specified message event consumer
        /// </summary>
        /// <param name="services">This Service Collection</param>
        /// <param name="eventConsumerConfiguration">A class that implements the <see cref="IConsumerConfiguration{TConsumer, TMessage}"/> to asynchronous configure the evet consumer options.</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQEventConsumer<TConsumer, TMessage>(this IServiceCollection services, IConsumerConfiguration<TConsumer, TMessage> eventConsumerConfiguration) where TConsumer : EventConsumer<TMessage> where TMessage : class, IEvent
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
                        if (eventConsumerConfiguration != null)
                        {
                            await eventConsumerConfiguration.ConfigureAsync(sp, st);
                        }
                    }
                };

                return result;
            });
            services.AddSingleton<IConsumerManager<TConsumer>, EventConsumerManager<TConsumer, TMessage>>();

            return services;
        }


        /// <summary>
        /// Register the specified message event consumer
        /// </summary>
        /// <param name="services">This Service Collection</param>
        /// <param name="configureBuilder">Action to configure the consumer builder.</param>
        /// <param name="configureOptions">Action to configure the consumer options.</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQBasicConsumer<TConsumer, TMessage>(this IServiceCollection services, Action<BasicConsumerServiceBuilder<TConsumer, TMessage>> configureBuilder, Action<IServiceProvider, ConsumerOptions> configureOptions) where TConsumer : BasicConsumer<TMessage> where TMessage : class
        {
            services.AddScoped<TConsumer>();
            services.AddSingleton<IConsumerService, BasicConsumerService<TConsumer, TMessage>>((serviceProvider) =>
            {
#if NETCOREAPP2_1
                var hostEnvironment = serviceProvider.GetRequiredService<IHostingEnvironment>();
#else
                var hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
#endif
                var bus = serviceProvider.GetRequiredService<IBus>();
                var conventions = serviceProvider.GetRequiredService<IConventions>();
                var logger = serviceProvider.GetRequiredService<ILogger<BasicConsumerService<TConsumer, TMessage>>>();

                var result = new BasicConsumerService<TConsumer, TMessage>(serviceProvider, hostEnvironment, bus, conventions, logger)
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

                var builder = new BasicConsumerServiceBuilder<TConsumer, TMessage>(result);

                configureBuilder?.Invoke(builder);

                return result;
            });
            services.AddSingleton<IConsumerManager<TConsumer>, BasicConsumerManager<TConsumer, TMessage>>();

            return services;
        }

        /// <summary>
        /// Register the specified message event consumer
        /// </summary>
        /// <param name="services">This Service Collection</param>
        /// <param name="configureBuilder">Action to configure the consumer builder.</param>
        /// <param name="configureOptions">Function to asynchronous configure the consumer options.</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQBasicConsumer<TConsumer, TMessage>(this IServiceCollection services, Action<BasicConsumerServiceBuilder<TConsumer, TMessage>> configureBuilder, Func<IServiceProvider, ConsumerOptions, Task> configureOptions) where TConsumer : BasicConsumer<TMessage> where TMessage : class
        {
            services.AddScoped<TConsumer>();
            services.AddSingleton<IConsumerService, BasicConsumerService<TConsumer, TMessage>>((serviceProvider) =>
            {
#if NETCOREAPP2_1
                var hostEnvironment = serviceProvider.GetRequiredService<IHostingEnvironment>();
#else
                var hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
#endif
                var bus = serviceProvider.GetRequiredService<IBus>();
                var conventions = serviceProvider.GetRequiredService<IConventions>();
                var logger = serviceProvider.GetRequiredService<ILogger<BasicConsumerService<TConsumer, TMessage>>>();

                var result = new BasicConsumerService<TConsumer, TMessage>(serviceProvider, hostEnvironment, bus, conventions, logger)
                {
                    ConfigureOptions = async (sp, st) =>
                    {
                        if (configureOptions != null)
                        {
                            await configureOptions.Invoke(sp, st);
                        }
                    }
                };

                var builder = new BasicConsumerServiceBuilder<TConsumer, TMessage>(result);

                configureBuilder?.Invoke(builder);

                return result;
            });
            services.AddSingleton<IConsumerManager<TConsumer>, BasicConsumerManager<TConsumer, TMessage>>();

            return services;
        }

        /// <summary>
        /// Register the specified message consumer
        /// </summary>
        /// <param name="services">This Service Collection</param>
        /// <param name="configureBuilder">Action to configure the consumer builder.</param>
        /// <param name="eventConsumerConfiguration">A class that implements the <see cref="IConsumerConfiguration{TConsumer, TMessage}"/> to asynchronous configure the evet consumer options.</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQBasicConsumer<TConsumer, TMessage>(this IServiceCollection services, Action<BasicConsumerServiceBuilder<TConsumer, TMessage>> configureBuilder, IConsumerConfiguration<TConsumer, TMessage> eventConsumerConfiguration) where TConsumer : BasicConsumer<TMessage> where TMessage : class
        {
            services.AddScoped<TConsumer>();
            services.AddSingleton<IConsumerService, BasicConsumerService<TConsumer, TMessage>>((serviceProvider) =>
            {
#if NETCOREAPP2_1
                var hostEnvironment = serviceProvider.GetRequiredService<IHostingEnvironment>();
#else
                var hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
#endif
                var bus = serviceProvider.GetRequiredService<IBus>();
                var conventions = serviceProvider.GetRequiredService<IConventions>();
                var logger = serviceProvider.GetRequiredService<ILogger<BasicConsumerService<TConsumer, TMessage>>>();

                var result = new BasicConsumerService<TConsumer, TMessage>(serviceProvider, hostEnvironment, bus, conventions, logger)
                {
                    ConfigureOptions = async (sp, st) =>
                    {
                        if (eventConsumerConfiguration != null)
                        {
                            await eventConsumerConfiguration.ConfigureAsync(sp, st);
                        }
                    }
                };

                var builder = new BasicConsumerServiceBuilder<TConsumer, TMessage>(result);

                configureBuilder?.Invoke(builder);

                return result;
            });
            services.AddSingleton<IConsumerManager<TConsumer>, BasicConsumerManager<TConsumer, TMessage>>();

            return services;
        }
    }
}
