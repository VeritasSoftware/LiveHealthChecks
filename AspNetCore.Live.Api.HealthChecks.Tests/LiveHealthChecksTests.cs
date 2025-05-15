using AspNetCore.Live.Api.HealthChecks.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Sample.Server;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            //Start the Sample Server
            var serverWebHostBuilder = new WebHostBuilder()
                                        .UseStartup<Startup>()
                                        .UseKestrel(options => options.ListenAnyIP(5001, listenOptions => 
                                                        listenOptions.UseHttps(o => o.AllowAnyClientCertificate())));

            serverWebHostBuilder.Start();

            //Start the Sample Api
            var apiWebHostBuilder = new WebHostBuilder()
                                        .UseStartup<Sample.Api.Startup>()
                                        .UseKestrel(options => options.ListenAnyIP(5000, listenOptions =>
                                                        listenOptions.UseHttps(o => o.AllowAnyClientCertificate())));

            var app = apiWebHostBuilder.Build();
            await app.StartAsync();            

            //Start Monitoring app connection to Server
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

            var receivedHealthReportStr = string.Empty;

            //Listen for set ReceiveMethod.
            connection.On<string>("SampleApiHealth", healthReport =>
            {
                //Receive health report from Server
                receivedHealthReportStr = healthReport;
            });

            await connection.StartAsync();

            Thread.Sleep(2000);

            //Api publishes health report to Server
            //Get IMyHealthCheckService service from Api container
            var myHealthCheckService = (IMyHealthCheckService)app.Services.GetService(typeof(IMyHealthCheckService));
            //Run health check on Api
            var publishedHealthReport = await myHealthCheckService.CheckHealthAsync();
            //Act - Publish health report to Server
            await myHealthCheckService.PublishHealthReportAsync(publishedHealthReport);

            var options = new JsonSerializerOptions();
            options.Converters.Add(new IntPtrConverter());
            options.Converters.Add(new SystemTextJsonExceptionConverter());
            options.WriteIndented = true;
            var publishedHealthReportStr = JsonSerializer.Serialize(publishedHealthReport, options);

            while (string.IsNullOrWhiteSpace(receivedHealthReportStr))
            {
                Thread.Sleep(50);
            }

            //Assert
            Assert.Equal(publishedHealthReportStr, receivedHealthReportStr);
        }
    }

    public class IntPtrConverter : JsonConverter<IntPtr>
    {
        public override IntPtr Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new IntPtr(reader.GetInt64());
        }

        public override void Write(Utf8JsonWriter writer, IntPtr value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.ToInt64());
        }
    }

    public class SystemTextJsonExceptionConverter : JsonConverter<Exception>
    {
        public override Exception Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(nameof(Exception.Message), value.Message);

            if (value.InnerException is not null)
            {
                writer.WriteStartObject(nameof(Exception.InnerException));
                Write(writer, value.InnerException, options);
                writer.WriteEndObject();
            }

            if (value.TargetSite is not null)
            {
                writer.WriteStartObject(nameof(Exception.TargetSite));
                writer.WriteString(nameof(Exception.TargetSite.Name), value.TargetSite?.Name);
                writer.WriteString(nameof(Exception.TargetSite.DeclaringType), value.TargetSite?.DeclaringType?.FullName);
                writer.WriteEndObject();
            }

            if (value.StackTrace is not null)
            {
                writer.WriteString(nameof(Exception.StackTrace), value.StackTrace);
            }

            writer.WriteString(nameof(Type), value.GetType().ToString());
            writer.WriteEndObject();
        }
    }

}