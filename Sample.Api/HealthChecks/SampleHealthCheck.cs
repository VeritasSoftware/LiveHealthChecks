﻿using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Sample.Api.HealthChecks
{
    public class SampleHealthCheck : IHealthCheck
    {
        //Generate random health check status
        private Random _random = new Random();

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var td = (int)DateTime.UtcNow.Ticks;

            _random = new Random(td);

            var seed = _random.Next();

            var random = new Random(seed);
            var num = random.Next(1, 100);

            var result = new HealthCheckResult(num % 2 == 0 ? HealthStatus.Healthy : HealthStatus.Unhealthy);

            return Task.FromResult(result);
        }
    }
}
