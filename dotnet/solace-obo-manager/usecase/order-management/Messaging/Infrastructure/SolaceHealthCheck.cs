using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Messaging.Infrastructure
{
    // Infrastructure/SolaceHealthCheck.cs
    public class SolaceHealthCheck(SolaceConnection connection) : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken ct)
        {
            try
            {
                var session = connection.Session;
                return Task.FromResult(HealthCheckResult.Healthy("Solace connected"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    HealthCheckResult.Unhealthy("Solace not connected", ex));
            }
        }
    }
}
