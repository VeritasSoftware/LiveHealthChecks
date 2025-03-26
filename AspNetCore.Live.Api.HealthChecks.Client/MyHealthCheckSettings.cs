using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspNetCore.Live.Api.HealthChecks.Client
{
    public class MyHealthCheckSettings
    {
        public int? HealthCheckIntervalInMinutes { get; set; } = 15;
        public string? HealthCheckIntervalCronExpression { get; set; }
        public string HealthCheckServerHubUrl { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ReceiveMethod { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public bool PublishOnlyWhenNotHealthy { get; set; }
        public Func<HealthReport, object>? TransformHealthReport { get; set; } = null;
        public bool AddHealthCheckMiddleware { get; set; } = true;
    }
}
