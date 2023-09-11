using AspNetCore.Live.Api.HealthChecks.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Sample.Server;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCore.Live.Api.HealthChecks.Tests
{
    public class LiveHealthChecksTests
    {
        [Fact]
        public async Task Publish_Receive_Pass()
        {
            //Arrange
            //The message to be published.
            var message = new MyHealthCheckModel
            {                
                ReceiveMethod = "SampleApiHealth",
                SecretKey = "43bf0968-17e0-4d22-816a-6eaadd766692",
                ClientId = "SampleApi",
                Report = "{\"Entries\":{},\"Status\":2,\"TotalDuration\":\"00:00:00.0015278\"}",
            };            

            //Start the Sample Server
            var webHostBuilder = new WebHostBuilder()
                                        .UseStartup<Startup>()
                                        .UseKestrel(options => options.ListenAnyIP(5001, listenOptions => 
                                                        listenOptions.UseHttps(o => o.AllowAnyClientCertificate())));

            webHostBuilder.Start();


            //Start Api connection to Server
            var apiConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/livehealthcheckshub", o =>
                {
                    o.Headers.Add("LiveHealthChecks-ReceiveMethod", "SampleApiHealth");
                    o.Headers.Add("LiveHealthChecks-SecretKey", "43bf0968-17e0-4d22-816a-6eaadd766692");
                    //Optional - value can be anything you want. good for tracking in the logs.
                    o.Headers.Add("LiveHealthChecks-ClientId", "Sample Api");
                })
                .WithAutomaticReconnect()
                .Build();

            await apiConnection.StartAsync();

            //Start monitoring app connection to Server
            var connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/livehealthcheckshub", o =>
                {
                    o.Headers.Add("LiveHealthChecks-ReceiveMethod", "*");
                    o.Headers.Add("LiveHealthChecks-SecretKey", "f22f3fd2-687d-48a1-aa2f-f2c9181364eb");
                    //Optional - value can be anything you want. good for tracking in the logs.
                    o.Headers.Add("LiveHealthChecks-ClientId", "Monitoring App 1");
                })
                .WithAutomaticReconnect()
                .Build();

            var receivedHealthReport = string.Empty;

            //Listen for set ReceiveMethod.
            connection.On<string>("SampleApiHealth", healthReport =>
            {
                //Receive health report from Server
                receivedHealthReport = healthReport;
            });

            await connection.StartAsync();

            //Api publishes message to Server
            //Act
            await apiConnection.InvokeAsync("PublishMyHealthCheck", message);

            while(string.IsNullOrWhiteSpace(receivedHealthReport))
            {
                Thread.Sleep(50);
            }

            //Assert
            Assert.Equal(message.Report, receivedHealthReport);
        }
    }
}