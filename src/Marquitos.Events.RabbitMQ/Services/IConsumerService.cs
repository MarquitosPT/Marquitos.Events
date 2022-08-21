using System.Threading;
using System.Threading.Tasks;

namespace Marquitos.Events.RabbitMQ.Services
{
    internal interface IConsumerService
    {
        Task StartAsync(CancellationToken cancellationToken = default);
        
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
