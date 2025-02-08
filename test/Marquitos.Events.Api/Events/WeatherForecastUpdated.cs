namespace Marquitos.Events.Api.Events
{
    public class WeatherForecastUpdated : IEvent
    {
        /// <summary>
        /// The weather forecast just updated
        /// </summary>
        public WeatherForecast WeatherForecast { get; set; } = null!;
    }
}
