using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Sample.Api.HealthChecks
{
    public class IsAliveHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("https://localhost:7277/api/healthcheck/isalive");
                if (response.IsSuccessStatusCode)
                {
                    return await Task.FromResult(HealthCheckResult.Healthy("The API is alive and reachable."));
                }
                else
                {
                    return await Task.FromResult(HealthCheckResult.Unhealthy("The API is not reachable."));
                }
            }
        }
    }
}
