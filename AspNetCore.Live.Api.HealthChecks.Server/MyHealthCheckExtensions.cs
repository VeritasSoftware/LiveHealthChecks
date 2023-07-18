namespace AspNetCore.Live.Api.HealthChecks.Server
{
    public static class MyHealthCheckExtensions
    {
        public static IServiceCollection AddLiveHealthChecksServer(this IServiceCollection services, Action<MyHealthCheckSettings>? settings = null)
        {
            var mySettings = new MyHealthCheckSettings();

            settings?.Invoke(mySettings);

            services.AddSingleton(sp => mySettings);

            return services;
        }
    }
}
