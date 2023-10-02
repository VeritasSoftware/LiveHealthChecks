namespace LiveHealthChecks.UI.Models
{
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
}
