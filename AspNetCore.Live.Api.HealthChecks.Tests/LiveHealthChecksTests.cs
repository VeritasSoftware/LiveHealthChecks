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
            var message = new MyHealthCheckModel
            {                
                ReceiveMethod = "SampleApiHealth",
                SecretKey = "43bf0968-17e0-4d22-816a-6eaadd766692",
                ClientId = "SampleApi",
                Report = "{\"Entries\":{},\"Status\":2,\"TotalDuration\":\"00:00:00.0015278\"}",
            };

            var echo = string.Empty;
            var webHostBuilder = new WebHostBuilder()
                                        .UseStartup<Startup>()
                                        .UseKestrel(options => options.ListenAnyIP(5001, listenOptions => 
                                                        listenOptions.UseHttps(o => o.AllowAnyClientCertificate())));

            webHostBuilder.Start();

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

            connection.On<string>("SampleApiHealth", msg =>
            {
                echo = msg;
            });

            await connection.StartAsync();

            //Act
            await connection.InvokeAsync("PublishMyHealthCheck", message);

            while(string.IsNullOrWhiteSpace(echo))
            {
                Thread.Sleep(50);
            }

            //Assert
            Assert.Equal(message.Report, echo);
        }
    }
}