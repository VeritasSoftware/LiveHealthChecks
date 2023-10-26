using Blazored.LocalStorage;
using LiveHealthChecks.UI;
using LiveHealthChecks.UI.Models;
using LiveHealthChecks.UI.Repository;
using LiveHealthChecks.UI.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddLogging();

var serviceProvider = builder.Services.BuildServiceProvider();
var logger = serviceProvider.GetService<ILogger<ApplicationLogs>>();
builder.Services.AddSingleton(typeof(ILogger), logger);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddMudServices();

builder.Services.AddScoped<IMyHealthChecksRepository, MyHealthChecksRepository>();
builder.Services.AddScoped<IMyDialogService, MyDialogService>();

await builder.Build().RunAsync();
