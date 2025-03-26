using MongoDB.Driver;

namespace AspNetCore.Live.Api.HealthChecks.Server
{
    public static class MyHealthCheckExtensions
    {
        public static IServiceCollection AddLiveHealthChecksServer(this IServiceCollection services, Action<MyHealthCheckSettings>? settings = null)
        {
            var mySettings = new MyHealthCheckSettings();

            settings?.Invoke(mySettings);

            services.AddSingleton(sp => mySettings);

            if (mySettings.UseDatabase)
            {
                if (mySettings.Configure != null)
                {
                    services.AddSingleton<IMongoClient, MongoClient>(mySettings.Configure);
                }
                else
                {
                    services.AddSingleton<IMongoClient, MongoClient>(sp => new MongoClient(mySettings.DatabaseConnectionString));
                }
                services.AddScoped<IServerRepository, ServerRepository>();
            }

            return services;
        }
    }
}
