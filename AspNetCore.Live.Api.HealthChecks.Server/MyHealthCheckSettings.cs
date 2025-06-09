using MongoDB.Driver;
using System.Text.RegularExpressions;

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

        public bool UseCustomDatabase { get; set; } = false;

        public string? DatabaseConnectionString { get; set; }

        public string DatabaseName
        {
            get
            {
                var m = Regex.Match(this.DatabaseConnectionString!, @"^.*/(?<dbName>.+)$");

                return m.Success ? m.Groups["dbName"].Captures[0].Value : "ServerDb";
            }
        }

        public Func<IServiceProvider, MongoClient>? Configure { get; set; }        
    }
}
