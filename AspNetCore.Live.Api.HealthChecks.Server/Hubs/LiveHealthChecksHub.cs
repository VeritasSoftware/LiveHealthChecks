using Microsoft.AspNetCore.SignalR;

namespace AspNetCore.Live.Api.HealthChecks.Server.Hubs
{
    public class LiveHealthChecksHub : Hub
    {
        private readonly MyHealthCheckSettings _settings;
        private readonly ILogger<LiveHealthChecksHub>? _logger;

        public LiveHealthChecksHub(MyHealthCheckSettings settings, ILogger<LiveHealthChecksHub>? logger = null)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task PublishMyHealthCheck(MyHealthCheckModel myHealthCheck)
        {
            try
            {
                if (string.Compare(_settings.SecretKey, myHealthCheck.SecretKey, false) == 0)
                {
                    _logger?.LogInformation($"Sending health check report to {myHealthCheck.ReceiveMethod}. Connection Id: {base.Context.ConnectionId}.");

                    await base.Clients.All.SendAsync(myHealthCheck.ReceiveMethod, myHealthCheck.Report, new object());

                    _logger?.LogInformation($"Sent health check report to {myHealthCheck.ReceiveMethod}. Connection Id: {base.Context.ConnectionId}.");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error in {nameof(LiveHealthChecksHub)}.");
            }                      
        }
    }
}
