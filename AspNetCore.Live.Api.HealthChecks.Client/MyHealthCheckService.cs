using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspNetCore.Live.Api.HealthChecks.Client
{
    public class MyHealthCheckService : BackgroundService
    {
        private readonly MyHealthCheckSettings _settings;
        private readonly IMyHealthCheckPublisher _publisher;
        private readonly ILogger<MyHealthCheckService>? _logger;
        private readonly HealthCheckService? _healthCheckService;

        public MyHealthCheckService(MyHealthCheckSettings settings, 
                                    IMyHealthCheckPublisher publisher,
                                    ILogger<MyHealthCheckService>? logger = null,
                                    HealthCheckService? healthCheckService = null)
        {
            _settings = settings;
            _publisher = publisher;
            _logger = logger;
            _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken = default)
        {
            try
            {
                await RunHealthCheckAndPublishHealthReport(stoppingToken);

                TimeSpan interval = TimeSpan.FromMinutes(_settings.HealthCheckIntervalInMinutes);
                using PeriodicTimer timer = new PeriodicTimer(interval);

                while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
                {
                    try
                    {
                        await RunHealthCheckAndPublishHealthReport(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, $"Error in {nameof(MyHealthCheckService)}.");
                    }                   
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error in {nameof(MyHealthCheckService)}.");
            }            
        }

        private async Task RunHealthCheckAndPublishHealthReport(CancellationToken stoppingToken)
        {            
            if (_healthCheckService != null)
            {
                _logger?.LogInformation("Running Health Check.");

                var report = await _healthCheckService.CheckHealthAsync(stoppingToken);

                _logger?.LogInformation("Finished running Health Check.");

                if (_settings.PublishOnlyWhenNotHealthy)
                {
                    if (report.Status != HealthStatus.Healthy)
                    {
                        _logger?.LogInformation("Publishing Health Check.");

                        await _publisher.Publish(report);

                        _logger?.LogInformation("Finished publishing Health Check.");
                    }
                }
                else
                {
                    _logger?.LogInformation("Publishing Health Check.");

                    await _publisher.Publish(report);

                    _logger?.LogInformation("Finished publishing Health Check.");
                }
            }            
        }
    }
}