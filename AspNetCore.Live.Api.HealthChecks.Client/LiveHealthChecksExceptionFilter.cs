using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspNetCore.Live.Api.HealthChecks.Client
{
    public class LiveHealthChecksExceptionFilter : IAsyncExceptionFilter
    {
        private readonly IMyHealthCheckService _healthCheckService;
        private readonly ILogger<LiveHealthChecksExceptionFilter>? _logger;

        public LiveHealthChecksExceptionFilter(IMyHealthCheckService healthCheckService, 
                                                ILogger<LiveHealthChecksExceptionFilter>? logger = null)
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            try
            {
                _logger?.LogError(context.Exception, "LiveHealthChecks: An exception occurred.");

                await _healthCheckService.PublishExceptionHealthReportAsync(context.Exception);

                var healthReport = await _healthCheckService.CheckHealthAsync();

                _logger?.LogInformation("LiveHealthChecks: Publishing health report.");

                await _healthCheckService.PublishHealthReportAsync(healthReport);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"LiveHealthChecks: An exception occurred in {nameof(LiveHealthChecksExceptionFilter)}.");
            }            
        }
    }

    public class FilterRemovalProvider : IFilterProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public int Order => -1000; // Run early

        public FilterRemovalProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void OnProvidersExecuting(FilterProviderContext context)
        {
            var settings = _serviceProvider.GetRequiredService<MyHealthCheckSettingsHolder>().Current;

            if (settings.AddHealthCheckMiddleware)
            {
                return;
            };

            // Remove all instances of the target filter type
            var toRemove = context.Results
                .Where(r => r.Filter is LiveHealthChecksExceptionFilter)
                .ToList();

            foreach (var item in toRemove)
            {
                context.Results.Remove(item);
            }
        }

        public void OnProvidersExecuted(FilterProviderContext context)
        {
            // No-op
        }
    }
}
