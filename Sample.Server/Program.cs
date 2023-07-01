// See https://aka.ms/new-console-template for more information
using AspNetCore.Live.Api.HealthChecks.Server;
using AspNetCore.Live.Api.HealthChecks.Server.Hubs;

Console.WriteLine("Hello, Server!");
Console.WriteLine(Environment.NewLine);

var builder = WebApplication.CreateBuilder();

builder.Services.AddSignalR();
builder.Services.AddLiveHealthChecksServer(settings => settings.Clients = new ClientSettings[]
{
    new ClientSettings
    {
        ReceiveMethod = "SampleApiHealth",
        SecretKey = "43bf0968-17e0-4d22-816a-6eaadd766692"
    },
    //Optional
    //Monitoring app connecting with ReceiveMethod *
    //will receive notifications for all ReceiveMethods in the system.
    new ClientSettings {
        ReceiveMethod = "*",
        SecretKey = "f22f3fd2-687d-48a1-aa2f-f2c9181364eb"
    }
});

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<LiveHealthChecksHub>("/livehealthcheckshub");
});

app.Run();
