# LiveHealthChecks
# Real-Time Api Health Check Monitoring

|Packages|Version & Downloads|
|---------------------------|:---:|
|*AspNetCore.Live.Api.HealthChecks.Server*|[![NuGet Version and Downloads count](https://buildstats.info/nuget/AspNetCore.Live.Api.HealthChecks.Server)](https://www.nuget.org/packages/AspNetCore.Live.Api.HealthChecks.Server)|
|*AspNetCore.Live.Api.HealthChecks.Client*|[![NuGet Version and Downloads count](https://buildstats.info/nuget/AspNetCore.Live.Api.HealthChecks.Client)](https://www.nuget.org/packages/AspNetCore.Live.Api.HealthChecks.Client)|

## Background

An Asp Net Core Web Api has a [**Health Checks**](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-6.0) system built into it.

This project taps into that system & makes the generated [**Health Report**](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.diagnostics.healthchecks.healthreport?view=dotnet-plat-ext-6.0),

available to Monitoring applications, in real-time.



The **Client** package, installed in the Api, runs the **Health Check** periodically,

and uploads the generated Health Report to the **Server SignalR Hub**.

The Hub sends a web socket push notification to the connected clients,

notifying them of the Health Report in real-time.

![**LiveHealthChecks**](/Docs/SequenceDiagram.png)

## Server

You can use a Console app as a Health Checks Server.

Just create one with Web Sdk (project file):

```C#
<Project Sdk="Microsoft.NET.Sdk.Web">
```

Then, plug in the Server package.

```C#
var builder = WebApplication.CreateBuilder();

builder.Services.AddSignalR();
builder.Services.AddLiveHealthChecksServer(settings => settings.SecretKey = "43bf0968-17e0-4d22-816a-6eaadd766692");

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<LiveHealthChecksHub>("/livehealthcheckshub");
});

app.Run();
```

## Asp Net Core Api

In your Api add the Client Nuget package.

then

```C#
//Required - add all your health checks
services.AddHealthChecks();

services.AddLiveHealthChecksClient(settings =>
{
    settings.HealthCheckIntervalInMinutes = 60;
    settings.ReceiveMethod = "SampleApiHealth";
    settings.HealthCheckServerHubUrl = "https://localhost:5001/livehealthcheckshub";
    settings.SecretKey = "43bf0968-17e0-4d22-816a-6eaadd766692";
    settings.PublishOnlyWhenNotHealthy = false;
});
```
The **ReceiveMethod** is the SignalR method that Monitoring app needs to listen to.

The **SecretKey** must be the same between Server & Api.

Set **PublishOnlyWhenNotHealthy** to **true** if you want to publish anomalies,

ie those Health Reports with **not Healthy** status.

The Server sends the Health Report as a real-time push notification.

**Note:-** You can host a Server & Client in the same Api too. 

## Monitoring app

In your Monitoring app, create a SignalR connection to the Server Hub.

Then, start listening to the set **ReceiveMethod** ie "SampleApiHealth".

```C#
var connection = new HubConnectionBuilder()
                        .WithUrl("https://localhost:5001/livehealthcheckshub")
                        .WithAutomaticReconnect()
                        .Build();

connection.On("SampleApiHealth", new Type[] {typeof(object), typeof(object)},
    (arg1, arg2) =>
    {
        Console.WriteLine(arg1[0]);
        return Task.CompletedTask;
    }, new object());

await connection.StartAsync();
```

![**Sample Monitoring App**](/Docs/MonitoringApp.jpg)