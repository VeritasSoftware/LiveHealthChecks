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
                // If a custom repository is provided, use it; otherwise, use the default MongoDB Repository.
                if (mySettings.MyServerRepository != null)
                {
                    services.AddScoped(mySettings.MyServerRepository);
                }
                else
                {
                    // Default MongoDB repository
                    if (mySettings.Configure != null)
                    {
                        services.AddSingleton<IMongoClient, MongoClient>(mySettings.Configure);
                    }
                    else
                    {
                        services.AddSingleton<IMongoClient, MongoClient>(sp => new MongoClient(mySettings.DatabaseConnectionString));
                    }

                    services.AddScoped<IServerRepository, MyMongoDbServerRepository>();
                }                               
            }

            return services;
        }
    }
}
