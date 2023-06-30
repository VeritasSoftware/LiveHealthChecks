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

        public override async Task OnConnectedAsync()
        {            
            var receiveMethod = Context.GetHttpContext()?.Request.Headers["LiveHealthChecks-ReceiveMethod"].ToString();
            var secretKey = Context.GetHttpContext()?.Request.Headers["LiveHealthChecks-SecretKey"].ToString();

            var client = _settings.Clients?.SingleOrDefault(c => c.ReceiveMethod == receiveMethod && c.SecretKey == secretKey);

            if (client == null)
            {
                throw new ApplicationException("Authorization failed.");
            }

            await base.OnConnectedAsync();
        }

        public async Task PublishMyHealthCheck(MyHealthCheckModel myHealthCheck)
        {
            try
            {
                var client = _settings.Clients?.SingleOrDefault(c => c.ReceiveMethod == myHealthCheck.ReceiveMethod);

                if (client == null)
                {
                    throw new ApplicationException($"Client Api with ReceiveMethod {myHealthCheck.ReceiveMethod} not found.");
                }

                if (string.Compare(client.SecretKey, myHealthCheck.SecretKey, false) == 0)
                {
                    _logger?.LogInformation($"Sending Health Report ({myHealthCheck.Report}) to {myHealthCheck.ReceiveMethod}. Connection Id: {base.Context.ConnectionId}.");

                    await base.Clients.All.SendAsync(myHealthCheck.ReceiveMethod, myHealthCheck.Report, new object());

                    _logger?.LogInformation($"Sent Health Report ({myHealthCheck.Report}) to {myHealthCheck.ReceiveMethod}. Connection Id: {base.Context.ConnectionId}.");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error in {nameof(LiveHealthChecksHub)}.");
                throw;
            }                      
        }
    }
}
