using LiveHealthChecks.UI.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace LiveHealthChecks.UI.Services
{
    public interface IMyServerService
    {
        Task<HubConnection> ConnectAsync(DashboardSettings dashboardSettings);
        ValueTask DisconnectAsync();
        void Subscribe(string receiveMethod, Func<string, Task> onHealthReportReceivedHandler);
    }

    public class MyServerService : IMyServerService
    {
        private HubConnection? _connection;

        public async Task<HubConnection> ConnectAsync(DashboardSettings dashboardSettings)
        {
            await DisconnectAsync();

            Console.WriteLine("Connecting to Server.");

            _connection = new HubConnectionBuilder()
                               .WithUrl(dashboardSettings!.ServerUrl)
                               .WithAutomaticReconnect()
                               .Build();

            await _connection.StartAsync();

            await _connection.SendAsync("AuthenticateAsync", new MyHealthCheckAuthModel
            {
                ReceiveMethod = dashboardSettings.ServerReceiveMethod,
                SecretKey = dashboardSettings.ServerSecretKey,
                ClientId = dashboardSettings.ServerClientId
            });           

            return _connection;
        }

        public async ValueTask DisconnectAsync()
        {
            if (_connection != null)
            {
                if (_connection.State == HubConnectionState.Connected)
                {
                    Console.WriteLine("Disconnecting from Server.");
                    await _connection.SendAsync("DisconnectAsync");
                }

                Console.WriteLine("Disposing connection.");
                await _connection.DisposeAsync();
                _connection = null;
            }
        }

        public void Subscribe(string receiveMethod, Func<string, Task> onHealthReportReceivedHandler)
        {
            _connection?.On<string>(receiveMethod, onHealthReportReceivedHandler);  
        }
    }
}
