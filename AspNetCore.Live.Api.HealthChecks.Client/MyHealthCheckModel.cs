namespace AspNetCore.Live.Api.HealthChecks.Client
{
    public class MyHealthCheckModel
    {
        public string ClientId { get; set; } = string.Empty;
        public string ReceiveMethod { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Report { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
