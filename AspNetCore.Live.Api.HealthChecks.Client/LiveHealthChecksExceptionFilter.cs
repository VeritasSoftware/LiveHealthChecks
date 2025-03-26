using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspNetCore.Live.Api.HealthChecks.Client
{
    internal class LiveHealthChecksExceptionFilter : IAsyncExceptionFilter
    {
        private readonly IMyHealthCheckService _healthCheckService;

        public LiveHealthChecksExceptionFilter(IMyHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
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

            await _healthCheckService.PublishHealthReportAsync(exceptionReport);

            var healthReport = await _healthCheckService.CheckHealthAsync();

            await _healthCheckService.PublishHealthReportAsync(healthReport);
        }
    }
}
