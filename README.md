# LiveHealthChecks
# Real-Time Api Health Check Monitoring

|Packages|Version & Downloads|
|---------------------------|:---:|
|*AspNetCore.Live.Api.HealthChecks.Server*|[![NuGet Version and Downloads count](https://buildstats.info/nuget/AspNetCore.Live.Api.HealthChecks.Server)](https://www.nuget.org/packages/AspNetCore.Live.Api.HealthChecks.Server)|
|*AspNetCore.Live.Api.HealthChecks.Client*|[![NuGet Version and Downloads count](https://buildstats.info/nuget/AspNetCore.Live.Api.HealthChecks.Client)](https://www.nuget.org/packages/AspNetCore.Live.Api.HealthChecks.Client)|

<a name="TOC"/>

## Table of Contents

*  [Background](#Background)
*  [System Architecture](#SystemArchitecture)
*  [Server](#Server)
*  [Asp Net Core Api](#AspNetCoreApi)
*  [Real-Time Monitoring app](#Monitoringapp)
*  [Live - Trigger & publish Health Checks](#Trigger-Publish)

[Table of Contents](#TOC)

<a name="Background"/>

## Background

An Asp Net Core Web Api has a [**Health Checks**](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-6.0) system built into it.

This project taps into that system & makes the generated [**Health Report**](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.diagnostics.healthchecks.healthreport?view=dotnet-plat-ext-6.0),

available to Monitoring applications, in real-time.



The **Client** package, installed in the Api, runs the **Health Check** periodically,

and uploads the generated Health Report to the **Server SignalR Hub**.

The Hub sends a web socket push notification to the connected clients,

notifying them of the Health Report in real-time.

![**LiveHealthChecks**](/Docs/SequenceDiagram.png)

[Table of Contents](#TOC)

<a name="SystemArchitecture"/>

## System Architecture

The system can comprise of multiple APIs & multiple Monitoring Apps.

All connecting to the same Server Hub.

Each **Api Client** has a **ReceiveMethod** & **SecretKey**.

The ReceiveMethod is the method, the Monitoring apps have to listen to.

All Health Reports of that Api are published to this ReceiveMethod.

The **Server** has to be set up for each Api Client's ReceiveMethod & SecretKey.

When a **connection request** is made to the Server, the ReceiveMethod & SecretKey have to be provided in **custom headers**.

All connections to the Server (from the Api Client & Monitoring apps) are **authorized** using ReceiveMethod & SecretKey.

![**LiveHealthChecks-SystemArchitecture**](/Docs/SystemArchitecture.jpg)

[Table of Contents](#TOC)

<a name="Server"/>

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

//Load the Clients dynamically
builder.Services.AddScoped<IClientsService, ClientsService>();
builder.Services.AddLiveHealthChecksServer(options =>
{
    //Optional - Save Health Check info with Report to MongoDB database.
    //Set UseDatabase flag to true.
    //Provide the MongoDB connection string.
    options.UseDatabase = true;
    options.DatabaseConnectionString = "mongodb://localhost:27017/ServerDb";
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

The Server sends push notification to the ReceiveMethod, if the Client's SecretKey matches that on the Server.

You implement Server interface **IClientsService** to get the list of Clients.

The list can be stored in a database table (for eg.).

You could fetch the list from the database & cache it.

This way you do not need a Server shutdown to add a new Client Api to the system.

### Sample ClientsService

```C#
    public class ClientsService : IClientsService
    {
        public async Task<ClientSettings[]> GetClientsAsync()
        {
            //The Clients list below is hard-coded but,
            //You can fetch the Clients from a database (for eg.) and
            //You can cache the Clients too.
            return await Task.FromResult(new ClientSettings[]
            {
                new ClientSettings
                {
                    ReceiveMethod = "SampleApiHealth",
                    SecretKey = "43bf0968-17e0-4d22-816a-6eaadd766692"
                },
                new ClientSettings
                {
                    ReceiveMethod = "SampleApi2Health",
                    SecretKey = "ae6f9a48-259b-4d03-9956-a2bf8838aaa4"
                },
                //Optional
                //Monitoring app connecting with ReceiveMethod *
                //will receive notifications for all ReceiveMethods in the system.
                new ClientSettings {
                    ReceiveMethod = "*",
                    SecretKey = "f22f3fd2-687d-48a1-aa2f-f2c9181364eb"
                }
            });
        }
    }
```

![**Sample Server**](/Docs/Server.jpg)

[Table of Contents](#TOC)

<a name="AspNetCoreApi"/>

## Asp Net Core Api

In your Api add the Client Nuget package.

then

```C#
builder.Services.AddHealthChecks() //Required - add all your health checks
                .AddLiveHealthChecksClient(settings =>
                {
                    //You can set the health check interval
                    //by a Cron Expression. 
                    settings.HealthCheckIntervalCronExpression = "0 * * * *";
                    //Or in minutes
                    //settings.HealthCheckIntervalInMinutes = 60;
                    //Providing ClientId is optional. Good for tracking in the logs.
                    settings.ClientId = "SampleApi";
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

[Table of Contents](#TOC)

<a name="Monitoringapp"/>

## Real-Time Monitoring App

If you want to receive notifications for all **ReceiveMethods** in the system, on the same connection,

set the **ReceiveMethod** to * & use the **SecretKey** set in the Server.

The **ClientId** is optional, but useful in the logs.

```C#
var Connection = new HubConnectionBuilder()
                    .WithUrl("https://localhost:5001/livehealthcheckshub")
                    .WithAutomaticReconnect()
                    .Build();


Connection.On<string>("SampleApiHealth", report =>
{
    //Handle report here
});

Connection.On<string>("SampleApi2Health", report =>
{
    //Handle report here
});

await Connection.StartAsync();

await Connection.SendAsync("AuthenticateAsync", new
{
    ReceiveMethod = "*",
    SecretKey = "f22f3fd2-687d-48a1-aa2f-f2c9181364eb",
    ClientId = "Monitoring App"
});  
```

If you want to receive notification from a specific Api,

you can Authenticate with that Api's **ReceiveMethod** & **SecretKey**.

The **ClientId** is optional, but useful in the logs.

```C#
var connection = new HubConnectionBuilder()
                        .WithUrl("https://localhost:5001/livehealthcheckshub")
                        .WithAutomaticReconnect()
                        .Build();

connection.On<string>("SampleApiHealth", report =>
{
    Console.WriteLine(report);
});

await connection.StartAsync();

await Connection.SendAsync("AuthenticateAsync", new
{
    ReceiveMethod = "SampleApiHealth",
    SecretKey = "43bf0968-17e0-4d22-816a-6eaadd766692",
    ClientId = "SampleApi"
});
```

To **Disconnect** example:

```C#
await Connection.SendAsync("DisconnectAsync");
await Connection.DisposeAsync(); 
```

### Sample

I have provided a sample real-time health checks monitoring web app.

The sample web app is containerized & there is an docker image you can download from **DockerHub**.

![**Sample Monitoring web app - LiveHealthChecks.UI**](/Docs/LiveHealthChecks-UI.jpg)

[**LiveHealthChecks.UI**](Docs/README_LiveHealthChecks.UI.md)

[Table of Contents](#TOC)

<a name="Trigger-Publish"/>

## Live - Trigger & publish Health Checks

Besides, the Client package running the Health Check on the Api itself, periodically,

you can run a Health Check and publish the Health Report to the Server.


You can trigger a Health Check, at any point, from anywhere, in your API,

by injecting the Client package's **IMyHealthCheckService** interface and,

calling the **CheckHealthAsync** method.


This method is a wrapper around the built-in Health Check system's [**HealthCheckService**](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.diagnostics.healthchecks.healthcheckservice?view=dotnet-plat-ext-6.0).

and then, publish the generated Health Report to the Server yourself,

by calling the **PublishHealthReportAsync** method.

![**IMyHealthCheckService**](/Docs/IMyHealthCheckService.png)

[Table of Contents](#TOC)