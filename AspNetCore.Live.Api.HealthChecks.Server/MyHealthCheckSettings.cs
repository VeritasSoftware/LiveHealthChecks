namespace AspNetCore.Live.Api.HealthChecks.Server
{
    public class ClientSettings
    {
        public string ReceiveMethod { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
    }

    public class MyHealthCheckSettings
    {
        public ClientSettings[]? Clients {  get; set; }

        public bool UseDatabase { get; set; } = false;

        public string? DatabaseConnectionString { get; set; }
    }
}
