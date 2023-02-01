namespace Marquitos.Events.RabbitMQ.Services
{
    internal interface IConsumerService
    {
        bool IsEnabled { get; }

        bool IsConsuming { get; }

        Task InitializeAsync(CancellationToken cancellationToken = default);

        Task StartAsync(CancellationToken cancellationToken = default);

        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
