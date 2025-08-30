# LiveHealthChecks
# Real-Time Api/App Health Check Monitoring

### Appreciate a star (*) on the repository, if it worked for you.

|Packages|Nuget Version|Downloads|
|---------------------------|:---:|:---:|
|*AspNetCore.Live.Api.HealthChecks.Server*|[![Nuget Version](https://img.shields.io/nuget/v/AspNetCore.Live.Api.HealthChecks.Server)](https://www.nuget.org/packages/AspNetCore.Live.Api.HealthChecks.Server)|[![Downloads count](https://img.shields.io/nuget/dt/AspNetCore.Live.Api.HealthChecks.Server)](https://www.nuget.org/packages/AspNetCore.Live.Api.HealthChecks.Server)|
|*AspNetCore.Live.Api.HealthChecks.Client*|[![Nuget Version](https://img.shields.io/nuget/v/AspNetCore.Live.Api.HealthChecks.Client)](https://www.nuget.org/packages/AspNetCore.Live.Api.HealthChecks.Client)|[![Downloads count](https://img.shields.io/nuget/dt/AspNetCore.Live.Api.HealthChecks.Client)](https://www.nuget.org/packages/AspNetCore.Live.Api.HealthChecks.Client)|

### The Nuget packages support .NET 6/7/8/9.

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

The generated health report is a JSON object, which contains the status of all the health checks in the Api.

```json
{
  "Entries": {
    "Is Alive Health Check Api 1": {
      "Data": {},
      "Description": "The API is alive and reachable.",
      "Duration": "00:00:00.0646557",
      "Exception": null,
      "Status": 2,
      "Tags": []
    },
    "Sample Health Check Api 1": {
      "Data": {},
      "Description": null,
      "Duration": "00:00:00.0004172",
      "Exception": null,
      "Status": 0,
      "Tags": []
    }
  },
  "Status": 0,
  "TotalDuration": "00:00:00.0648796"
}
```

The **Client** package, installed in the Api, runs the **Health Check** periodically,

and uploads the generated Health Report to the **Server SignalR Hub**.

The Hub sends a web socket push notification to the connected Monitoring apps,

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

When a Monitoring app makes a **connection request** to the Server, the ReceiveMethod & SecretKey have to be provided when you **Authenicate**.

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
    //Optional - Save Health Check info with Report to database.
    //Set UseDatabase flag to true.    
    options.UseDatabase = true;

    //If you want to use a custom database
    //Provide your own implementation of IServerRepository.
    //options.UseCustomDatabase = true;

    //Default database - MongoDB
    //Provide the MongoDB connection string.
    options.DatabaseConnectionString = "mongodb://localhost:27017/ServerDb";
    //Optional - Configure MongoClient.
    //options.Configure = sp => new MongoClient(options.DatabaseConnectionString);  
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

You can create a **special Client account** with ReceiveMethod of * and a SecretKey.

This account can be used by Monitoring apps that want to get notifications for all Apis in the system,

on the same SignalR connection.

**Note :-** If you have any Web monitoring apps connecting to the Server, set up **CORS** for them.

Please see the Sample Server's Starup.cs to get an idea on how to do that.

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
                //will receive notifications for all ReceiveMethods in the system on the same connection.
                new ClientSettings {
                    ReceiveMethod = "*",
                    SecretKey = "f22f3fd2-687d-48a1-aa2f-f2c9181364eb"
                }
            });
        }
    }
```

### Server database

By default, the Server persists Health Report data in a **MongoDB** database.

If you want, you can **persist the data to any database of your choice**.

<img src="/Docs/ServerRepository.png" alt="Third-party database" width="400px" height="250px">

Just implement interface `IServerRepository` and create your own class (eg `SQLServerRepository`) that talks to any database.

Persist the `model` in your interface method implementation.

Set the settings property `UseCustomDatabase` to `true`.

```C#
options.UseCustomDatabase = true;
```

Wire up your implementation class for dependency injection. Eg.

```C#
services.AddScoped<IServerRepository>(sp => new SQLServerRepository(...));
```

### Sample

![**Sample Server**](/Docs/Server.jpg)

[Table of Contents](#TOC)

<a name="AspNetCoreApi"/>

## Asp Net Core Api

In your Api add the Client Nuget package.

then

```C#
builder.Services.AddHealthChecks() //Required - add all your health checks
                .AddCheck<IsAliveHealthCheck>("Is Alive Health Check Api 1")
                .AddCheck<SampleHealthCheck>("Sample Health Check Api 1")
                //Add Live Health Checks Client
                .AddLiveHealthChecksClient(settings =>
                {
                    //You can set the health check interval
                    //by a Cron Expression. 
                    settings.HealthCheckIntervalCronExpression = "* * * * *";
                    //Or in minutes
                    //settings.HealthCheckIntervalInMinutes = 1;
                    settings.HealthCheckServerHubUrl = "https://localhost:5001/livehealthcheckshub";                    
                    settings.ReceiveMethod = "SampleApiHealth";                    
                    settings.SecretKey = "43bf0968-17e0-4d22-816a-6eaadd766692";
                    //Providing ClientId is optional. Good for tracking in the logs.
                    settings.ClientId = "Sample Api";
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
                    settings.AddHealthCheckMiddleware = true;
                });
```
The **ReceiveMethod** is the SignalR method that Monitoring app needs to listen to.

The **SecretKey** must be the same between Server & Api.

Set **PublishOnlyWhenNotHealthy** to **true** if you want to publish anomalies,

ie those Health Reports with **not Healthy** [**status**](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.diagnostics.healthchecks.healthstatus?view=dotnet-plat-ext-6.0).

The Server sends the Health Report as a real-time push notification.

**Note:-** You can host a Server & Client in the same Api too. 

### Sample

![**Sample Api**](/Docs/Api.jpg)

[Table of Contents](#TOC)

<a name="Monitoringapp"/>

## Real-Time Monitoring App

The Monitoring App connects to the Server using a [**SignalR Client library**](https://learn.microsoft.com/en-us/aspnet/core/signalr/client-features?view=aspnetcore-8.0).

Then, the App authenticates & listens to the **ReceiveMethod** to start receiving push notifications.

If you want to receive notifications for all **ReceiveMethods** in the system, on the same connection,

use the special Client account with ReceiveMethod of *, set up on the Server.

The **ClientId** is optional, but useful in the logs.

**.NET C# sample**

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

**TypeScript sample**

```TypeScript
const signalr = require('@microsoft/signalr')
.
.
connection: any;

connection = new signalr.HubConnectionBuilder()
    .withUrl("https://localhost:5001/livehealthcheckshub")
    .build();

connection.on("SampleApiHealth", (report: any) => {
    //Handle report here
});

connection.on("SampleApi2Health", (report: any) => {
    //Handle report here
});

connection.start()
.then(() => connection.invoke("AuthenticateAsync",
{
    ReceiveMethod : "*",
    SecretKey : "f22f3fd2-687d-48a1-aa2f-f2c9181364eb",
    ClientId : "Monitoring App"
}));
```

If you want to receive notification from a **specific Api**,

you can Authenticate with that Api's **ReceiveMethod** & **SecretKey**.

The **ClientId** is optional, but useful in the logs.

**.NET C# sample**

```C#
Connection.On<string>("SampleApiHealth", report =>
{
    //Handle report here
});

await Connection.StartAsync();

await Connection.SendAsync("AuthenticateAsync", new
{
    ReceiveMethod = "SampleApiHealth",
    SecretKey = "43bf0968-17e0-4d22-816a-6eaadd766692",
    ClientId = "Monitoring app"
});
```

**TypeScript sample**

```TypeScript
connection.on("SampleApiHealth", (report: any) => {
    //Handle report here
});

connection.start()
.then(() => connection.invoke("AuthenticateAsync",
{
    ReceiveMethod : "SampleApiHealth",
    SecretKey : "43bf0968-17e0-4d22-816a-6eaadd766692",
    ClientId : "Monitoring App"
}));
```

### Disconnect example:

**.NET C# sample**

```C#
await Connection.SendAsync("DisconnectAsync");
await Connection.DisposeAsync(); 
```

**TypeScript sample**

```TypeScript
if (connection != null) {
    connection.invoke("DisconnectAsync");
    connection.off("SampleApiHealth");
    connection.off("SampleApi2Health");
    connection = null;
}
```

### Samples

I have provided a sample real-time health checks monitoring web app.

The sample web app is containerized & there is an docker image you can download from **DockerHub**.

#### Certificate files

You may need to replace the **certificate files** (in the `/etc/nginx` folder of the docker container) with your own, if they have expired.

You can use the `docker cp` command to copy the new certificate files into the container.

The new certificate should have the same name as the existing one.

The certificate files needed to be replaced are:

- `livehealthchecks.ui.crt`
- `livehealthchecks.ui.key`
- `livehealthchecks.ui.pem`

![**Sample Monitoring web app - LiveHealthChecks.UI**](/Docs/LiveHealthChecks-UI.jpg)

#### Blazor Web Assembly sample app

[**Blazor - LiveHealthChecks.UI**](Docs/README_LiveHealthChecks.UI.md)

#### React sample app

[**React - LiveHealthChecks.UI**](Docs/README_React_LiveHealthChecks.UI.md)

[Table of Contents](#TOC)

<a name="Trigger-Publish"/>

## Live - Trigger & publish Health Checks

Besides, the Client package running the Health Check on the Api itself, periodically,

you can run a Health Check and publish the Health Report to the Server.


You can trigger a Health Check, at any point (eg. on the occurance of a specific Exception), from anywhere, in your API,

by injecting the Client package's [**IMyHealthCheckService**](/AspNetCore.Live.Api.HealthChecks.Client/MyHealthCheckService.cs) interface and,

calling the **CheckHealthAsync** method.

```C#
public interface IMyHealthCheckService
{
    Task<HealthReport> CheckHealthAsync(CancellationToken stoppingToken = default);
    Task PublishHealthReportAsync(HealthReport report);
    Task PublishExceptionHealthReportAsync(Exception exception);
}
```

This method is a wrapper around the built-in Health Check system's [**HealthCheckService**](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.diagnostics.healthchecks.healthcheckservice?view=dotnet-plat-ext-6.0).

and then, publish the generated Health Report to the Server yourself, by calling the **PublishHealthReportAsync** method.

You can also call the **PublishExceptionHealthReportAsync** method, to publish an Exception as a Health Report.

This has **machine diagnostic** implications too.

Eg. In your app, you can trap certain types of database exceptions,

and in the error handler, publish the Exception Health Report and/or you can trigger the health checks & publish the Health Report,

to the Server & on to the Monitoring apps,

in **real-time**.

So, this way you can learn the internal state of your app & the machine in real-time.


### Live Health Checks Middleware

You can set property **AddHealthCheckMiddleware** to **true** in the Client settings,

to add a middleware to the Api, which will add an [**Exception Filter**](/AspNetCore.Live.Api.HealthChecks.Client/LiveHealthChecksExceptionFilter.cs), 

that will publish an **ExceptionHealthReport** & trigger the Health Checks & publish the Health Report,

in case of an unhandled exception, in real-time.

[![**Exception Filter**](/Docs/ExceptionFilter.jpg)](/AspNetCore.Live.Api.HealthChecks.Client/LiveHealthChecksExceptionFilter.cs)

[Table of Contents](#TOC)