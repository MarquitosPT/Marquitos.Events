using Marquitos.Events.Api.Events;
using Marquitos.Events.RabbitMQ.Consumers;

namespace Marquitos.Events.Api.Consumers
{
    public class WeatherForecastUpdatedConsumer : EventConsumer<WeatherForecastUpdated>
    {
        public override async Task HandleMessageAsync(WeatherForecastUpdated message, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Received an message!");
            await Task.CompletedTask;
        }
    }
}
