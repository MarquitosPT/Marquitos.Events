using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Marquitos.Events.RabbitMQ.Services
{
    internal class RabbitConsumerService : IHostedService
    {
        private readonly ILogger<RabbitConsumerService> _logger;
        private readonly IEnumerable<IConsumerService> _services;

        public RabbitConsumerService(ILogger<RabbitConsumerService> logger, IEnumerable<IConsumerService> services)
        {
            _logger = logger;
            _services = services;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("{Service} is starting.", nameof(RabbitConsumerService));

            foreach (var item in _services)
            {
                await item.InitializeAsync(cancellationToken);
            }

            _logger.LogInformation("{Service} has started.", nameof(RabbitConsumerService));
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("{Service} is stopping.", nameof(RabbitConsumerService));

            foreach (var item in _services)
            {
                await item.StopAsync(cancellationToken);
            }

            _logger.LogInformation("{Service} has stopped.", nameof(RabbitConsumerService));

            await Task.Delay(100);
        }
    }
}
