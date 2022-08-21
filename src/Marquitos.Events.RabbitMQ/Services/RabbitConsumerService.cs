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
            _logger.LogDebug("{Service} is starting.", nameof(RabbitConsumerService));

            foreach (var item in _services)
            {
                await item.StartAsync(cancellationToken);
            }

            _logger.LogDebug("{Service} has started.", nameof(RabbitConsumerService));
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("{Service} is stopping.", nameof(RabbitConsumerService));

            foreach (var item in _services)
            {
                await item.StopAsync(cancellationToken);
            }

            _logger.LogDebug("{Service} has stopped.", nameof(RabbitConsumerService));

            await Task.Delay(100);
        }
    }
}
