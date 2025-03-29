using Microsoft.AspNetCore.SignalR;

namespace AspNetCore.Live.Api.HealthChecks.Server.Hubs
{
    public class LiveHealthChecksHub : Hub
    {
        private static List<LoggedInUser> _loggedInUsers = new List<LoggedInUser>();

        private readonly MyHealthCheckSettings _settings;
        private readonly IClientsService? _clientsService;
        private readonly ILogger<LiveHealthChecksHub>? _logger;
        private readonly IServerRepository? _repository;

        public LiveHealthChecksHub(MyHealthCheckSettings settings, IClientsService? clientsService = null, ILogger<LiveHealthChecksHub>? logger = null, IServerRepository? repository = null)
        {
            _settings = settings;
            _clientsService = clientsService;
            _logger = logger;
            _repository = repository;
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

                bool isAuthenticatedSeparately = false;

                if (!httpContext.Request.Headers.ContainsKey("LiveHealthChecks-ReceiveMethod"))
                {
                    _logger?.LogInformation($"Request does not contain LiveHealthChecks-ReceiveMethod header. Authorization deffered. ConnectionId: {Context.ConnectionId}");
                    isAuthenticatedSeparately = true;
                }

                if (!isAuthenticatedSeparately)
                {
                    string clientId = string.Empty;
                    if (httpContext.Request.Headers.TryGetValue("LiveHealthChecks-ClientId", out var cId))
                    {
                        clientId = cId.ToString();
                    }
                    var receiveMethod = httpContext.Request.Headers["LiveHealthChecks-ReceiveMethod"].ToString();
                    var secretKey = httpContext.Request.Headers["LiveHealthChecks-SecretKey"].ToString();
                   
                    await AuthenticateAsync(new MyHealthCheckBaseModel
                    {
                        ClientId = clientId,
                        ReceiveMethod = receiveMethod,
                        SecretKey = secretKey
                    });
                }                

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Server Error in {nameof(LiveHealthChecksHub)}.");
                throw;
            }            
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                if (exception != null)
                {
                    _logger?.LogError(exception, "Server Error");
                }

                await DisconnectAsync();

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Server Error in {nameof(LiveHealthChecksHub)}.");
                throw;
            }
        }

        public async Task AuthenticateAsync(MyHealthCheckBaseModel model)
        {
            try
            {
                string clientId = model.ClientId;
                var receiveMethod = model.ReceiveMethod;
                var secretKey = model.SecretKey;

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

                var client = (_clientsService != null ? await _clientsService.GetClientsAsync() : _settings.Clients)?.SingleOrDefault(c => c.ReceiveMethod == receiveMethod && c.SecretKey == secretKey);

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
            }           
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Server Error in {nameof(LiveHealthChecksHub)}.");
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
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

                    await Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Server Error: Removing logged in user failed.");
                }
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
                var client = (_clientsService != null ? await _clientsService.GetClientsAsync() : _settings.Clients)?.SingleOrDefault(c => c.ReceiveMethod == myHealthCheck.ReceiveMethod);

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

                    if (_settings.UseDatabase && _repository != null)
                    {
                        _logger?.LogInformation($"Saving Health Check to the database.");

                        await _repository.AddHealthCheckAsync(
                            new MyHealthCheckDbModel
                            {
                                ClientId = myHealthCheck.ClientId,
                                ReceiveMethod = myHealthCheck.ReceiveMethod,
                                Report = myHealthCheck.Report,
                                Timestamp = myHealthCheck.Timestamp
                            });

                        _logger?.LogInformation($"Saved Health Check to the database.");
                    }
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
