using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace AspNetCore.Live.Api.HealthChecks.Client
{
    public interface IMyHealthCheckPublisher
    {
        Task PublishAsync(HealthReport healthReport);
    }

    public class MyHealthCheckPublisher : IMyHealthCheckPublisher
    {
        private readonly MyHealthCheckSettings _settings;
        private readonly ILogger<MyHealthCheckPublisher>? _logger;
        private readonly IServiceProvider _serviceProvider;

        public MyHealthCheckPublisher(IServiceProvider serviceProvider, ILogger<MyHealthCheckPublisher>? logger = null)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task PublishAsync(HealthReport healthReport)
        {
            var settings = new JsonSerializerOptions
            {
                Converters = { new SystemTextJsonExceptionConverter() },
                WriteIndented = true
            };

            var healthChecksettings = _serviceProvider.GetRequiredService<MyHealthCheckSettingsHolder>().Current;

            bool isTransform = healthChecksettings.TransformHealthReport != null;
            string? publishedReport = null;

            if(isTransform)
            {
                publishedReport = JsonSerializer.Serialize(healthChecksettings.TransformHealthReport!(healthReport), settings);
            }
            else
            {
                publishedReport = JsonSerializer.Serialize(healthReport, typeof(HealthReport), settings);
            }            
            
            var connection = MyHealthCheckExtensions._healthChecksHubConnection;

            if (connection == null)
            {
                _logger?.LogError("The Server Hub connection is null.");
                return;
            }

            if (connection.State != HubConnectionState.Connected)
                await connection.StartAsync();
            
            _logger?.LogInformation($"Published Health Report: {publishedReport}, ReceiveMethod: {healthChecksettings.ReceiveMethod}, ClientId: {healthChecksettings.ClientId}");

            var request = new MyHealthCheckModel
            {
                ClientId = healthChecksettings.ClientId,
                ReceiveMethod = healthChecksettings.ReceiveMethod,
                SecretKey = healthChecksettings.SecretKey,
                Report = publishedReport
            };

            await connection.InvokeAsync("PublishMyHealthCheck", request);
        }
    }
}
