using AspNetCore.Live.Api.HealthChecks.Client;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthChecksBuilderExtensions
    {
        public static IHealthChecksBuilder AddLiveHealthChecksClient(this IHealthChecksBuilder builder, Action<MyHealthCheckSettings> settings)
        {
            builder.Services.AddLiveHealthChecksClient(settings);
            return builder;
        }
    }
}
