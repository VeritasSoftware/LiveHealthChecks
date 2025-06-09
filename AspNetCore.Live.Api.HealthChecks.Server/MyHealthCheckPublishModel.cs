using MongoDB.Bson;

namespace AspNetCore.Live.Api.HealthChecks.Server
{
    public class MyHealthCheckBaseModel
    {
        public string ClientId { get; set; } = string.Empty;
        public string ReceiveMethod { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
    }

    public class MyHealthCheckPublishModel : MyHealthCheckBaseModel
    {     
        public string Report { get; set; } = string.Empty;
        public DateTime? Timestamp { get; set; }
    }

    public class MyHealthCheckMongoDbModel
    {
        public ObjectId Id { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public string ReceiveMethod { get; set; } = string.Empty;
        public string Report { get; set; } = string.Empty;
        public DateTime? Timestamp { get; set; }
    }

    public class MyHealthCheckModel
    {
        public string ClientId { get; set; } = string.Empty;
        public string ReceiveMethod { get; set; } = string.Empty;
        public string Report { get; set; } = string.Empty;
        public DateTime? Timestamp { get; set; }
    }
}
