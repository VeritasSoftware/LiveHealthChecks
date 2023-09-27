namespace LiveHealthChecks.UI.Models
{
    public class HealthReport
    {
        public string Api { get; set; } = string.Empty;
        public string ReceiveMethod { get; set; } = string.Empty;
        public DateTime? ReceiveTimeStamp { get; set; } = null;
        public string Report { get; set; } = string.Empty;
        public double Status { get; set; }
    }

    public struct HealthStatus
    {
        public const double Unhealthy = 1.00;

        public const double Healthy = 2.00;
    }
}
