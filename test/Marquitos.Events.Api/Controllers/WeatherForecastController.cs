using Marquitos.Events.Api.Events;
using Marquitos.Events.Services;
using Microsoft.AspNetCore.Mvc;

namespace Marquitos.Events.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IEventService _eventService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IEventService eventService)
        {
            _logger = logger;
            _eventService = eventService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost(Name = "PostWeatherForecast")]
        public async Task<WeatherForecast> Post(WeatherForecast weatherForecast)
        {
            // TODO: save the model into a database service,
            // for testing purposes just notify the created event and return the same model.
            
            // Notify weather forecast created
            await _eventService.NotifyAsync(new WeatherForecastCreated() { WeatherForecast = weatherForecast });

            return weatherForecast;
        }
    }
}