namespace LiveHealthChecks.UI.Models
{
    public class MyHealthCheckAuthModel
    {
        public string ClientId { get; set; } = string.Empty;
        public string ReceiveMethod { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
    }

    public class MyApiHealthCheckModel
    {
        public string ApiName { get; set; } = string.Empty;
        public string ReceiveMethod { get; set; } = string.Empty;
    }

    public class DashboardSettings
    {
        public string ServerUrl { get; set; } = string.Empty;
        public string ServerReceiveMethod { get; set; } = string.Empty;
        public string ServerSecretKey { get; set; } = string.Empty;
        public string ServerClientId { get; set; } = string.Empty;
        public List<MyApiHealthCheckModel> Apis { get; set; } = new List<MyApiHealthCheckModel>();
    }

    public class HealthCheck
    {
        public string Api { get; set; } = string.Empty;
        public string ReceiveMethod { get; set; } = string.Empty;
        public DateTime? ReceiveTimeStamp { get; set; } = null;
        public HealthReport? Report { get; set; }
        public double Status { get; set; }
    }

    public struct HealthStatus
    {
        public const double Unhealthy = 1.00;

        public const double Healthy = 2.00;
    }

    public class HealthReport
    {
        public Status Status { get; set; }
        public string? TotalDuration { get; set; }
        public Dictionary<string, Status>? Entries { get; set; }
    }

    public enum Status
    {
        Unhealthy,
        Degraded,        
        Healthy
    }

    public class TransformedHealthReport
    {
        public string? Status { get; set; }
        public Dictionary<string, string>? Results { get; set; }
    }

    public class ApplicationLogs
    {
    }
}
