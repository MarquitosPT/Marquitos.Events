using Marquitos.Events.Api.Consumers;
using Marquitos.Events.Api.Events;
using Marquitos.Events.RabbitMQ;

namespace Marquitos.Events.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add event system services
            builder.Services.AddRabbitMQConnection(builder.Configuration.GetConnectionString("RabbitConnection"));
            builder.Services.AddRabbitMQEventService();
            builder.Services.AddRabbitMQConsumerService();
            builder.Services.AddRabbitMQEventConsumer<WeatherForecastCreatedConsumer, WeatherForecastCreated>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}