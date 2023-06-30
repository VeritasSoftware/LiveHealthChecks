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
            services.AddHostedService<MyHealthCheckService>();

            _healthChecksHubConnection = new HubConnectionBuilder()
                    .WithUrl(mySettings.HealthCheckServerHubUrl, o =>
                    {
                        o.Headers.Add("LiveHealthChecks-ReceiveMethod", mySettings.ReceiveMethod);
                        o.Headers.Add("LiveHealthChecks-SecretKey", mySettings.SecretKey);
                    })
                    .WithAutomaticReconnect()
                    .Build();

            return services;
        }
    }
}
