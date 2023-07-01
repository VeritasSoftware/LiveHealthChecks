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

## System Architecture

The system can comprise of multiple APIs & multiple Monitoring Apps.

All connecting to the same Server Hub.

Each **Api Client** has a **ReceiveMethod** & **SecretKey**.

The ReceiveMethod is the method, the Monitoring apps have to listen to.

All Health Reports of that Api are published to this ReceiveMethod.

The **Server** has to be set up for each Api Client. The ReceiveMethod & SecretKey provided.

The ReceiveMethod & SecretKey have to be provided in **custom headers** when the connection request is made.

All connections to the Server (from the Api Client & Monitoring apps) use this for **Authorization**.

![**LiveHealthChecks-SystemArchitecture**](/Docs/SystemArchitecture.jpg)

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
builder.Services.AddLiveHealthChecksServer(settings => settings.Clients = new ClientSettings[]
{
    new ClientSettings
    {
        ReceiveMethod = "SampleApiHealth",
        SecretKey = "43bf0968-17e0-4d22-816a-6eaadd766692"
    }
});

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<LiveHealthChecksHub>("/livehealthcheckshub");
});

app.Run();
```

A **Client (Api)** with a **ReceiveMethod** & **SecretKey** are set up in the Server.

The Api publishes to the Server with this information.

The Server sends push notification to the ReceiveMethod, if the Client's  SecretKey matches.

![**Sample Server**](/Docs/Server.jpg)

## Asp Net Core Api

In your Api add the Client Nuget package.

then

```C#
builder.Services.AddHealthChecks() //Required - add all your health checks
                .AddLiveHealthChecksClient(settings =>
                {
                    settings.HealthCheckIntervalInMinutes = 60;
                    settings.ReceiveMethod = "SampleApiHealth";
                    settings.HealthCheckServerHubUrl = "https://localhost:5001/livehealthcheckshub";
                    settings.SecretKey = "43bf0968-17e0-4d22-816a-6eaadd766692";
                    settings.PublishOnlyWhenNotHealthy = false;
                    //Optional - transform your health report to as you want it published.
                    settings.TransformHealthReport = healthReport => new
                    {
                        status = healthReport.Status.ToString(),
                        results = healthReport.Entries.Select(e => new
                        {
                            key = e.Key,
                            value = e.Value.Status.ToString()
                        })
                    };
                });
```
The **ReceiveMethod** is the SignalR method that Monitoring app needs to listen to.

The **SecretKey** must be the same between Server & Api.

Set **PublishOnlyWhenNotHealthy** to **true** if you want to publish anomalies,

ie those Health Reports with **not Healthy** [**status**](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.diagnostics.healthchecks.healthstatus?view=dotnet-plat-ext-6.0).

The Server sends the Health Report as a real-time push notification.

**Note:-** You can host a Server & Client in the same Api too. 

![**Sample Api**](/Docs/Api.jpg)

## Monitoring app

In your Monitoring app, create a SignalR connection to the Server Hub.

Then, start listening to the set **ReceiveMethod** ie "SampleApiHealth".

Set the **Headers** as shown. Use the same **ReceiveMethod** & **SecretKey**.

```C#
var connection = new HubConnectionBuilder()
                        .WithUrl("https://localhost:5001/livehealthcheckshub", o =>
                        {
                            o.Headers.Add("LiveHealthChecks-ReceiveMethod", "SampleApiHealth");
                            o.Headers.Add("LiveHealthChecks-SecretKey", "43bf0968-17e0-4d22-816a-6eaadd766692");
                        })
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

## Live - Trigger & publish Health Checks

Besides, the Client package running the Health Check on the Api itself, periodically,

you can run a Health Check and publish the Health Report to the Server.


You can trigger a Health Check, at any point, from anywhere, in your API,

by injecting the built-in Health Check system's' [**HealthCheckService**](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.diagnostics.healthchecks.healthcheckservice?view=dotnet-plat-ext-6.0),

and calling the **CheckHealthAsync** method.

and then, publish the generated Health Report to the Server yourself,

by injecting the Client package's '**IMyHealthCheckPublisher** interface and calling the **Publish** method.