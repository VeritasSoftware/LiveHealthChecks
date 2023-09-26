namespace AspNetCore.Live.Api.HealthChecks.Server
{
    public class MyHealthCheckBaseModel
    {
        public string ClientId { get; set; } = string.Empty;
        public string ReceiveMethod { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
    }

    public class MyHealthCheckModel : MyHealthCheckBaseModel
    {     
        public string Report { get; set; } = string.Empty;
    }
}
