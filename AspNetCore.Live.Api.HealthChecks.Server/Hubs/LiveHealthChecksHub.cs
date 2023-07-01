using Microsoft.AspNetCore.SignalR;

namespace AspNetCore.Live.Api.HealthChecks.Server.Hubs
{
    public class LiveHealthChecksHub : Hub
    {
        private static List<LoggedInUser> _loggedInUsers = new List<LoggedInUser>();

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

            if (string.IsNullOrEmpty(receiveMethod))
            {
                _logger?.LogError($"Authorization failed ReceiveMethod: {receiveMethod}, ConnectionId: {Context.ConnectionId}.");

                throw new ApplicationException("Authorization failed. Please provide ReceiveMethod.");
            }

            if (string.IsNullOrEmpty(secretKey))
            {
                _logger?.LogError($"Authorization failed ReceiveMethod: {receiveMethod}, ConnectionId: {Context.ConnectionId}.");

                throw new ApplicationException("Authorization failed. Please provide SecretKey.");
            }

            _logger?.LogInformation($"Authorizing ReceiveMethod: {receiveMethod}, ConnectionId: {Context.ConnectionId}.");

            var client = _settings.Clients?.SingleOrDefault(c => c.ReceiveMethod == receiveMethod && c.SecretKey == secretKey);

            if (client == null)
            {
                _logger?.LogError($"Authorization failed ReceiveMethod: {receiveMethod}, ConnectionId: {Context.ConnectionId}.");

                throw new ApplicationException("Authorization failed.");
            }

            if (!_loggedInUsers.Exists(u => u.ReceiveMethod == receiveMethod && u.ConnectionId == Context.ConnectionId))
            {
                lock(this)
                {
                    _loggedInUsers.Add(new LoggedInUser
                    {
                        ReceiveMethod = receiveMethod,
                        ConnectionId = Context.ConnectionId
                    });
                }                
            }            

            _logger?.LogInformation($"Authorized ReceiveMethod: {receiveMethod}, ConnectionId: {Context.ConnectionId}.");            

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception != null)
            {
                _logger?.LogError(exception, "Server Error");
            }

            try
            {
                lock(this)
                {
                    _logger?.LogInformation($"Logging out ConnectionId: {Context.ConnectionId}.");

                    _loggedInUsers.RemoveAll(u => u.ConnectionId == Context.ConnectionId);

                    _logger?.LogInformation($"Logged out ConnectionId: {Context.ConnectionId}.");
                }                
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Server Error: Removing logged in user failed.");
            }

            return base.OnDisconnectedAsync(exception);
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

                    var connectionIds = _loggedInUsers.Where(u => u.ReceiveMethod == myHealthCheck.ReceiveMethod)
                                                      .Select(u => u.ConnectionId)
                                                      .ToList();

                    await base.Clients.Clients(connectionIds).SendAsync(myHealthCheck.ReceiveMethod, myHealthCheck.Report, new object());

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
