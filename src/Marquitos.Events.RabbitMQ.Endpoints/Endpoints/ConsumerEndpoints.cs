using Marquitos.Events.RabbitMQ.Endpoints.Models;
using Marquitos.Events.RabbitMQ.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Linq;

namespace Marquitos.Events.RabbitMQ.Endpoints
{
    /// <summary>
    /// Represents the Consumer endpoints
    /// </summary>
    public static class ConsumerEndpoints
    {
        /// <summary>
        /// Maps the Consumer endpoints
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="requireAuthorization"></param>
        public static void MapRabbitMQConsumerEndpoints(this IEndpointRouteBuilder routes, bool requireAuthorization = false)
        {
            const string basePath = "/api/rabbitmq/consumers";
            const string tags = "RabbitMQ Consumers";

            var handlerGetAll = routes.MapGet(basePath, (IEnumerable<IConsumerService> consumerServices) =>
            {
                return consumerServices.Select(e => new Consumer
                {
                    Id = e.GetConsumerId(),
                    Name = e.GetConsumerName(),
                    MessageName = e.GetConsumerMessageName(),
                    QueueName = e.GetConsumerQueueName(),
                    Topics = e.GetConsumerTopics(),
                    HostName = e.GetConsumerHostName(),
                    IsConsuming = e.IsConsuming,
                    IsEnabled = e.IsEnabled
                });
            })
            .WithName("GetAllConsumers")
#if NET8_0_OR_GREATER
            .WithSummary("Get all consumers")
            .WithDescription("Returns a list of all consumers.") 
#endif
            .WithTags(tags)
            .Produces<List<Consumer>>(StatusCodes.Status200OK);

            var handlerGetById = routes.MapGet(basePath + "/{id}", (string id, IEnumerable<IConsumerService> consumerServices) =>
            {
                var result = consumerServices
                    .Where(e => e.GetConsumerId() == id)
                    .Select(e => new Consumer
                    {
                        Id = e.GetConsumerId(),
                        Name = e.GetConsumerName(),
                        MessageName = e.GetConsumerMessageName(),
                        QueueName = e.GetConsumerQueueName(),
                        Topics = e.GetConsumerTopics(),
                        HostName = e.GetConsumerHostName(),
                        IsConsuming = e.IsConsuming,
                        IsEnabled = e.IsEnabled
                    })
                    .FirstOrDefault();

                return result is not null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("GetConsumerById")
#if NET8_0_OR_GREATER
            .WithSummary("Get a consumer by id")
            .WithDescription("Returns the found consumer with the provided identification.")
#endif
            .WithTags(tags)
            .Produces<Consumer>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            var handlerStart = routes.MapPut(basePath + "/{id}/start", async (string id, IEnumerable<IConsumerService> consumerServices) =>
            {
                var result = consumerServices
                    .Where(e => e.GetConsumerId() == id)
                    .FirstOrDefault();

                if (result is null)
                {
                    return Results.NotFound();
                }

                if (result.IsEnabled is false)
                {
                    return Results.Conflict("Consumer is not enabled.");
                }

                if (!result.IsConsuming)
                {
                    await result.StartAsync();
                }

                return Results.NoContent();
            })
            .WithName("StartConsumer")
#if NET8_0_OR_GREATER
            .WithSummary("Start a consumer")
            .WithDescription("The found consumer starts consuming messages.")
#endif
            .WithTags(tags)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

            var handlerStop = routes.MapPut(basePath + "/{id}/stop", async (string id, IEnumerable<IConsumerService> consumerServices) =>
            {
                var result = consumerServices
                    .Where(e => e.GetConsumerId() == id)
                    .FirstOrDefault();

                if (result is null)
                {
                    return Results.NotFound();
                }

                if (result.IsEnabled is false)
                {
                    return Results.Conflict("Consumer is not enabled.");
                }

                if (result.IsConsuming)
                {
                    await result.StopAsync();
                }

                return Results.NoContent();
            })
            .WithName("StopConsumer")
#if NET8_0_OR_GREATER
            .WithSummary("Stop a consumer")
            .WithDescription("The found consumer stops consuming messages.")
#endif
            .WithTags(tags)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

            if (requireAuthorization)
            {
                handlerGetAll.RequireAuthorization();
                handlerGetById.RequireAuthorization();
                handlerStart.RequireAuthorization();
                handlerStop.RequireAuthorization();
            }
        }
    }
}
