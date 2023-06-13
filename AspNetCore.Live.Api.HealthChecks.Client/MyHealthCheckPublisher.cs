using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace AspNetCore.Live.Api.HealthChecks.Client
{
    public interface IMyHealthCheckPublisher
    {
        Task Publish(HealthReport healthReport);
    }

    public class MyHealthCheckPublisher : IMyHealthCheckPublisher
    {
        private readonly MyHealthCheckSettings _settings;

        public MyHealthCheckPublisher(MyHealthCheckSettings settings)
        {
            _settings = settings;
        }

        public async Task Publish(HealthReport healthReport)
        {
            var connection = MyHealthCheckExtensions._healthChecksHubConnection;

            if (connection == null)
            {
                return;
            }

            if (connection.State != HubConnectionState.Connected)
                await connection.StartAsync();

            var request = new MyHealthCheckModel
            {
                ReceiveMethod = _settings.ReceiveMethod,
                SecretKey = _settings.SecretKey,
                Report = JsonSerializer.Serialize(healthReport)
            };

            await connection.InvokeAsync("PublishMyHealthCheck", request);
        }
    }
}
