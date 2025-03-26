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

            services.AddSingleton(sp => mySettings);
            
            services.AddSingleton<IMyHealthCheckPublisher, MyHealthCheckPublisher>();
            services.AddSingleton<IMyHealthCheckService, MyHealthCheckService>();
            services.AddHostedService<MyHealthCheckBackgroundService>();

            if (mySettings.AddHealthCheckMiddleware)
            {
                services.AddMvc(o => o.Filters.Add<LiveHealthChecksExceptionFilter>());
            }

            _healthChecksHubConnection = new HubConnectionBuilder()
                    .WithUrl(mySettings.HealthCheckServerHubUrl, o =>
                    {
                        o.Headers.Add("LiveHealthChecks-ReceiveMethod", mySettings.ReceiveMethod);
                        o.Headers.Add("LiveHealthChecks-SecretKey", mySettings.SecretKey);

                        if (!string.IsNullOrEmpty(mySettings.ClientId))
                            o.Headers.Add("LiveHealthChecks-ClientId", mySettings.ClientId);
                    })
                    .WithAutomaticReconnect()
                    .Build();

            return services;
        }
    }
}
