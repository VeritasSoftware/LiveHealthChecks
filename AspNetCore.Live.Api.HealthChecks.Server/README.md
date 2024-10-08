# LiveHealthChecks
# Real-Time Api/App Health Check Monitoring

### Supports .NET 6/7/8.

|Packages|Version & Downloads|
|---------------------------|:---:|
|*AspNetCore.Live.Api.HealthChecks.Server*|[![Downloads count](https://img.shields.io/nuget/dt/AspNetCore.Live.Api.HealthChecks.Server)](https://www.nuget.org/packages/AspNetCore.Live.Api.HealthChecks.Server)|
|*AspNetCore.Live.Api.HealthChecks.Client*|[![Downloads count](https://img.shields.io/nuget/dt/AspNetCore.Live.Api.HealthChecks.Client)](https://www.nuget.org/packages/AspNetCore.Live.Api.HealthChecks.Client)|

An Asp Net Core Web Api has a [**Health Checks**](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-6.0) system built into it.

This project taps into that system & makes the generated [**Health Report**](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.diagnostics.healthchecks.healthreport?view=dotnet-plat-ext-6.0),

available to Monitoring applications, in real-time.

This **Server package** is used to build a Server which will direct traffic from the monitored Web API/App to the Monitoring applications.

## Documentation

Read [more](https://github.com/VeritasSoftware/LiveHealthChecks)