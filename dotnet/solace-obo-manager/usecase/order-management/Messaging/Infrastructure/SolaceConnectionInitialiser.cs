using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Messaging.Infrastructure
{
    // Infrastructure/SolaceConnectionInitialiser.cs
    public class SolaceConnectionInitialiser(
        SolaceConnection connection,
        ILogger<SolaceConnectionInitialiser> logger) : IHostedService
    {
        public async Task StartAsync(CancellationToken ct)
        {
            logger.LogInformation("Initialising Solace connection...");
            await connection.InitialiseAsync(ct);
            logger.LogInformation("Solace connection ready");
        }

        public Task StopAsync(CancellationToken ct)
        {
            logger.LogInformation("Solace connection stopping");
            connection.Dispose();
            return Task.CompletedTask;
        }
    }
}
