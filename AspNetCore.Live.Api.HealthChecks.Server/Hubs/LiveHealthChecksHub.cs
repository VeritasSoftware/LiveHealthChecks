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
            try
            {
                var httpContext = Context.GetHttpContext();

                if (httpContext == null)
                {
                    _logger?.LogError($"Authorization failed. HttpContext is null.");

                    throw new ApplicationException("Authorization failed. HttpContext is null. Server Error.");
                }

                string clientId = string.Empty;
                if (httpContext.Request.Headers.TryGetValue("LiveHealthChecks-ClientId", out var cId))
                {
                    clientId = cId.ToString();
                }
                var receiveMethod = httpContext.Request.Headers["LiveHealthChecks-ReceiveMethod"].ToString();
                var secretKey = httpContext.Request.Headers["LiveHealthChecks-SecretKey"].ToString();

                if (string.IsNullOrEmpty(receiveMethod))
                {
                    _logger?.LogError($"Authorization failed ReceiveMethod: {receiveMethod}, ConnectionId: {Context.ConnectionId}, ClientId: {clientId}.");

                    throw new ApplicationException("Authorization failed. Please provide ReceiveMethod.");
                }

                if (string.IsNullOrEmpty(secretKey))
                {
                    _logger?.LogError($"Authorization failed ReceiveMethod: {receiveMethod}, ConnectionId: {Context.ConnectionId}, ClientId: {clientId}.");

                    throw new ApplicationException("Authorization failed. Please provide SecretKey.");
                }

                _logger?.LogInformation($"Logging in ReceiveMethod: {receiveMethod}, ConnectionId: {Context.ConnectionId}, ClientId: {clientId}.");

                var client = _settings.Clients?.SingleOrDefault(c => c.ReceiveMethod == receiveMethod && c.SecretKey == secretKey);

                if (client == null)
                {
                    _logger?.LogError($"Authorization failed ReceiveMethod: {receiveMethod}, ConnectionId: {Context.ConnectionId}, ClientId: {clientId}.");

                    throw new ApplicationException("Authorization failed.");
                }

                if (!_loggedInUsers.Exists(u => u.ReceiveMethod == receiveMethod && u.ConnectionId == Context.ConnectionId))
                {
                    lock (this)
                    {
                        var loggedInUser = new LoggedInUser
                        {
                            ClientId = clientId,
                            ReceiveMethod = receiveMethod,
                            ConnectionId = Context.ConnectionId
                        };

                        Context.Items.Add(Context.ConnectionId, loggedInUser);

                        _loggedInUsers.Add(loggedInUser);

                        _logger?.LogInformation($"Logged in ReceiveMethod: {receiveMethod}, ConnectionId: {Context.ConnectionId}, ClientId: {clientId}.");
                    }
                }

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Server Error in {nameof(LiveHealthChecksHub)}.");
                throw;
            }            
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                if (exception != null)
                {
                    _logger?.LogError(exception, "Server Error");
                }

                try
                {
                    lock (this)
                    {
                        var loggedInUser = Context.Items[Context.ConnectionId] as LoggedInUser;

                        if (loggedInUser == null)
                        {
                            throw new ApplicationException("LoggedInUser details is null");
                        }

                        _logger?.LogInformation($"Logging out ConnectionId: {Context.ConnectionId}, ReceiveMethod: {loggedInUser.ReceiveMethod}, ClientId: {loggedInUser.ClientId}.");

                        _loggedInUsers.RemoveAll(u => u.ClientId == loggedInUser.ClientId
                                                        && u.ReceiveMethod == loggedInUser.ReceiveMethod
                                                        && u.ConnectionId == Context.ConnectionId);

                        _logger?.LogInformation($"Logged out ConnectionId: {Context.ConnectionId}, ReceiveMethod: {loggedInUser.ReceiveMethod}, ClientId: {loggedInUser.ClientId}.");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Server Error: Removing logged in user failed.");
                }

                return base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Server Error in {nameof(LiveHealthChecksHub)}.");
                throw;
            }
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
                    _logger?.LogInformation($"Sending Health Report ({myHealthCheck.Report}) to {myHealthCheck.ReceiveMethod}. Connection Id: {Context.ConnectionId}.");

                    var connectionIds = _loggedInUsers.Where(u => u.ReceiveMethod == "*" || u.ReceiveMethod == myHealthCheck.ReceiveMethod)
                                                      .Select(u => u.ConnectionId)
                                                      .ToList();

                    await base.Clients.Clients(connectionIds).SendAsync(myHealthCheck.ReceiveMethod, myHealthCheck.Report);

                    _logger?.LogInformation($"Sent Health Report ({myHealthCheck.Report}) to {myHealthCheck.ReceiveMethod}. Connection Id: {Context.ConnectionId}.");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Server Error in {nameof(LiveHealthChecksHub)}.");
                throw;
            }                      
        }
    }
}
