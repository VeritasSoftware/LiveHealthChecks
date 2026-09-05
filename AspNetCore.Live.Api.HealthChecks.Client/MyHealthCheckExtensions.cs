using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR.Client;

namespace AspNetCore.Live.Api.HealthChecks.Client
{
    public static class MyHealthCheckExtensions
    {
        internal static HubConnection? _healthChecksHubConnection = null;

        public static IServiceCollection AddLiveHealthChecksClient(this IServiceCollection services, Action<MyHealthCheckSettings> settings)
        {
            var mySettings = new MyHealthCheckSettings();

            settings(mySettings);

            var settingsHolder = new MyHealthCheckSettingsHolder(mySettings);

            services.AddSingleton(sp => settingsHolder);
            
            services.AddSingleton<IMyHealthCheckPublisher, MyHealthCheckPublisher>();
            services.AddSingleton<IMyHealthCheckService, MyHealthCheckService>();
            services.AddHostedService<MyHealthCheckBackgroundService>();

            if (mySettings.AddHealthCheckMiddleware)
            {
                services.AddMvc(o => o.Filters.Add<LiveHealthChecksExceptionFilter>());                
            }

            services.AddSingleton<IFilterProvider, FilterRemovalProvider>();

            services.AddControllers();

            settingsHolder.OnSettingsChanged += async (newSettings) =>
            {
                if (_healthChecksHubConnection != null)
                {
                    await _healthChecksHubConnection.StopAsync();
                    await _healthChecksHubConnection.DisposeAsync();
                    _healthChecksHubConnection = null;
                }

                BuildHealthChecksHubConnection(newSettings);
            };

            BuildHealthChecksHubConnection(mySettings);

            return services;
        }

        public static WebApplication UseLiveHealthChecksClient(this WebApplication app)
        {
            app.MapGet("/livehealthchecks/settings", (MyHealthCheckSettingsHolder holder) => new MyHealthCheckBasicSettings
            {
                HealthCheckIntervalCronExpression = holder.Current.HealthCheckIntervalCronExpression,
                HealthCheckIntervalInMinutes = holder.Current.HealthCheckIntervalInMinutes,
                HealthCheckServerHubUrl = holder.Current.HealthCheckServerHubUrl,
                PublishOnlyWhenNotHealthy = holder.Current.PublishOnlyWhenNotHealthy,
                AddHealthCheckMiddleware = holder.Current.AddHealthCheckMiddleware
            });

            app.MapPost("/livehealthchecks/settings/replace", (MyHealthCheckBasicSettings newSettings, [FromServices] MyHealthCheckSettingsHolder holder) =>
            {
                holder.Replace(newSettings);
                return Results.Ok("Settings replaced");
            });

            app.MapControllers();

            return app;
        }

        private static void BuildHealthChecksHubConnection(MyHealthCheckSettings settings)
        {
            _healthChecksHubConnection = new HubConnectionBuilder()
                .WithUrl(settings.HealthCheckServerHubUrl, o =>
                {
                    o.Headers.Add("LiveHealthChecks-ReceiveMethod", settings.ReceiveMethod);
                    o.Headers.Add("LiveHealthChecks-SecretKey", settings.SecretKey);
                    if (!string.IsNullOrEmpty(settings.ClientId))
                        o.Headers.Add("LiveHealthChecks-ClientId", settings.ClientId);
                })
                .WithAutomaticReconnect()
                .Build();
        }
    }
}
