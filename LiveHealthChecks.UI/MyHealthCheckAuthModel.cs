namespace LiveHealthChecks.UI
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
}
