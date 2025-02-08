[![NuGet Version](https://img.shields.io/nuget/v/Marquitos.Events.RabbitMQ)](https://www.nuget.org/packages/Marquitos.Events.RabbitMQ/)

# Marquitos.Events

A simple event system in top of RabbitMQ using EasyNetQ for AspNetCore applications.

# Usage
To create a consumer first create a class that descends of EventConsumer.
``` csharp
    public class ExampleConsumer : EventConsumer<ExampleCreated>
    {
        public override async Task HandleMessageAsync(ExampleCreated message, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Received an message!");
            await Task.CompletedTask;
        }
    }
```

Then register the RabbitMQ Consumer Service Engine and the Event Consumer on your services configuration:

``` csharp
...
    // Register the RabbitMQ connections string
    builder.Services.AddRabbitMQConnectionWithSystemTextJson(builder.Configuration.GetConnectionString("RabbitConnection"));

    // Register the Event Service to notify events (Optional) 
    builder.Services.AddRabbitMQEventService();

    // Register the Consumer Service Engine
    builder.Services.AddRabbitMQConsumerService();

    // Register your Consumer
    builder.Services.AddRabbitMQEventConsumer<ExampleConsumer, ExampleCreated>((sp, o) =>
    {
        // For example add two retry options
        o.Retries = new[] { 0.5, 1 }; // 30s and 1min
        o.IsEnabled = true;
    });
...
```
