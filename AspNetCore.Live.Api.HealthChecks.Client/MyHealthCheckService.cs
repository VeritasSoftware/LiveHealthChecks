using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspNetCore.Live.Api.HealthChecks.Client
{
    public interface IMyHealthCheckService
    {
        Task<HealthReport> CheckHealthAsync(CancellationToken stoppingToken = default);
        Task PublishHealthReportAsync(HealthReport report);
    }

    public class MyHealthCheckService : IMyHealthCheckService
    {
        private readonly ILogger<MyHealthCheckService>? _logger;
        private readonly MyHealthCheckSettings _settings;
        private readonly HealthCheckService _healthCheckService;
        private readonly IMyHealthCheckPublisher _healthCheckPublisher;

        public MyHealthCheckService(                                        
                                        MyHealthCheckSettings settings, 
                                        IMyHealthCheckPublisher healthCheckPublisher,
                                        HealthCheckService healthCheckService,
                                        ILogger<MyHealthCheckService>? logger = null
                                    )
        {
            _settings = settings;
            _healthCheckPublisher = healthCheckPublisher;
            _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));            
            _logger = logger;
        }

        public async Task PublishHealthReportAsync(HealthReport report)
        {
            if (_settings.PublishOnlyWhenNotHealthy)
            {
                if (report.Status != HealthStatus.Healthy)
                {
                    _logger?.LogInformation("Publishing Health Check.");

                    await _healthCheckPublisher.PublishAsync(report);

                    _logger?.LogInformation("Finished publishing Health Check.");
                }
            }
            else
            {
                _logger?.LogInformation("Publishing Health Check.");

                await _healthCheckPublisher.PublishAsync(report);

                _logger?.LogInformation("Finished publishing Health Check.");
            }
        }

        public async Task<HealthReport> CheckHealthAsync(CancellationToken stoppingToken = default)
        {
            _logger?.LogInformation("Triggering Health Check.");

            var report = await _healthCheckService.CheckHealthAsync(stoppingToken);

            _logger?.LogInformation("Finished running Health Check.");

            return report;
        }
    }
}
