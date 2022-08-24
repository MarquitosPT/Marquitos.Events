namespace Marquitos.Events.Api.Events
{
    public class WeatherForecastCreated : IEvent
    {
        /// <summary>
        /// The weather forecast just created
        /// </summary>
        public WeatherForecast WeatherForecast { get; set; }
    }
}
