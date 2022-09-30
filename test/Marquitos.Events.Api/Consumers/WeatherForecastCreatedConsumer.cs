using Marquitos.Events.Api.Events;
using Marquitos.Events.RabbitMQ.Consumers;

namespace Marquitos.Events.Api.Consumers
{
    public class WeatherForecastCreatedConsumer : EventConsumer<WeatherForecastCreated>
    {
        public override async Task<bool> InitializeAsync(EventConsumerOptions options, CancellationToken cancellationToken = default)
        {
            // TODO: Setup initial value options or get it from a database

            // For example add two retry options
            options.Retries = new[] { 0.5, 1 }; // 30s and 1min

            // Simply return true for this demo
            return await Task.FromResult(true);
        }

        public override async Task SetEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
        {
            // Not necessary for this demo
            
            await Task.CompletedTask;
        }
        public override Task HandleMessageAsync(WeatherForecastCreated message, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
