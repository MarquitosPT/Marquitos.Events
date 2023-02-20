using Marquitos.Events.Api.Events;
using Marquitos.Events.RabbitMQ.Consumers;

namespace Marquitos.Events.Api.Consumers
{
    public class WeatherForecastUpdatedConsumer : EventConsumer<WeatherForecastUpdated>
    {
        public override Task HandleMessageAsync(WeatherForecastUpdated message, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
