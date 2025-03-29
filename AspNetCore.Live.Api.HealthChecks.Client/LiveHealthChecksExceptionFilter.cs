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
            _logger?.LogError(context.Exception, "LiveHealthChecks: An exception occurred.");

            var exceptionReport = new HealthReport(new Dictionary<string, HealthReportEntry>
            {
                {
                    "ExceptionReport",
                    new HealthReportEntry(
                        HealthStatus.Unhealthy,
                        "An exception occurred.",
                        TimeSpan.Zero,
                        context.Exception,
                        new Dictionary<string, object>
                        {
                            { "StackTrace", (context.Exception.InnerException??context.Exception).StackTrace! }
                        }
                        )
                }
            }, TimeSpan.Zero);

            _logger?.LogInformation("LiveHealthChecks: Publishing exception report.");

            await _healthCheckService.PublishHealthReportAsync(exceptionReport);

            var healthReport = await _healthCheckService.CheckHealthAsync();

            _logger?.LogInformation("LiveHealthChecks: Publishing health report.");

            await _healthCheckService.PublishHealthReportAsync(healthReport);
        }
    }
}
