using Marquitos.Events.Api.Events;
using Marquitos.Events.RabbitMQ.Consumers;

namespace Marquitos.Events.Api.Consumers
{
    public class WeatherForecastCreatedConsumer : EventConsumer<WeatherForecastCreated>
    {
        public override Task HandleMessageAsync(WeatherForecastCreated message, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
