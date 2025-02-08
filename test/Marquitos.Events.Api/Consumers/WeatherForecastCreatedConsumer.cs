using Marquitos.Events.Api.Events;
using Marquitos.Events.RabbitMQ.Consumers;

namespace Marquitos.Events.Api.Consumers
{
    public class WeatherForecastCreatedConsumer : EventConsumer<WeatherForecastCreated>
    {
        public override async Task HandleMessageAsync(WeatherForecastCreated message, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Received an message!");
            await Task.CompletedTask;
        }
    }
}
