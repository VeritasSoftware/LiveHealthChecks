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

The **Server** has to be set up for each Api Client's ReceiveMethod & SecretKey.

When a **connection request** is made to the Server, the ReceiveMethod & SecretKey have to be provided in **custom headers**.

All connections to the Server (from the Api Client & Monitoring apps) are **authorized** using ReceiveMethod & SecretKey.

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

//Load the Clients dynamically
builder.Services.AddScoped<IClientsService, ClientsService>();
builder.Services.AddLiveHealthChecksServer();

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

## Monitoring web app

In your Monitoring web app, you call a Server Hub method called **AuthenticateAsync**.

And, before you close the Connection, call **DisconnectAsync**.


![**Sample Monitoring web app - LiveHealthChecks-UI**](/Docs/LiveHealthChecks-UI.jpg)


First, you configure the Monitoring web app, in a JSON file eg dashboardSettings.json.


```JSON
{
  "ServerUrl": "https://localhost:5001/livehealthcheckshub",
  "ServerReceiveMethod": "*",
  "ServerSecretKey": "f22f3fd2-687d-48a1-aa2f-f2c9181364eb",
  "ServerClientId": "LiveHealthChecks-UI",
  "Apis": [
    {
      "ApiName": "Sample Api",
      "ReceiveMethod": "SampleApiHealth"
    },
    {
      "ApiName": "Sample Api 2",
      "ReceiveMethod": "SampleApi2Health"
    }
  ]
}
```

If you want to receive notifications for all **ReceiveMethods** in the system, on the same connection,

set the **ServerReceiveMethod** header to * & use the SecretKey set in the Server.

Starting SignalR Connection & Authenticating example:

```C#
Connection = new HubConnectionBuilder()
                    .WithUrl(DashboardSettings.ServerUrl)
                    .WithAutomaticReconnect()
                    .Build();


connection.On<string>(DashboardSettings.Apis[0].ReceiveMethod, report =>
{
    //Handle report here
});

connection.On<string>(DashboardSettings.Apis[1].ReceiveMethod, report =>
{
    //Handle report here
});

await Connection.StartAsync();

await Connection.SendAsync("AuthenticateAsync", new
{
    ReceiveMethod = DashboardSettings.ServerReceiveMethod,
    SecretKey = DashboardSettings.ServerSecretKey,
    ClientId = DashboardSettings.ServerClientId
});  
```

To Disconnect example:

```C#
if (Connection != null)
{
    await Connection.SendAsync("DisconnectAsync");
    await Connection.DisposeAsync();
} 
```

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
                            //Optional - value can be anything you want. good for tracking in the logs.
                            o.Headers.Add("LiveHealthChecks-ClientId", "Monitoring App 1");
                        })
                        .WithAutomaticReconnect()
                        .Build();

connection.On<string>("SampleApiHealth", report =>
{
    Console.WriteLine(report);
});

await connection.StartAsync();
```

If you want to receive notifications for all **ReceiveMethods** in the system, on the same connection,

set the **ReceiveMethod** header to * & use the SecretKey set in the Server.


```C#
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

connection.On<string>("SampleApiHealth", report =>
{
    Console.WriteLine(report);
});

connection.On<string>("SampleApi2Health", report =>
{
    Console.WriteLine(report);
});

await connection.StartAsync();
```

![**Sample Monitoring App**](/Docs/MonitoringApp.jpg)

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