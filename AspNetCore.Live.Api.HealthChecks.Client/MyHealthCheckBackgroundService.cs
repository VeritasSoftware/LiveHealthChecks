using Cronos;

namespace AspNetCore.Live.Api.HealthChecks.Client
{
    public class MyHealthCheckBackgroundService : BackgroundService
    {
        private readonly MyHealthCheckSettings _settings;
        private readonly ILogger<MyHealthCheckBackgroundService>? _logger;
        private readonly IMyHealthCheckService _myHealthCheckService;

        public MyHealthCheckBackgroundService(
                                                MyHealthCheckSettings settings, 
                                                IMyHealthCheckService myHealthCheckService, 
                                                ILogger<MyHealthCheckBackgroundService>? logger = null
                                            )
        {            
            _settings = settings;
            _myHealthCheckService = myHealthCheckService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken = default)
        {
            try
            {
                await RunHealthCheckAndPublishHealthReport(stoppingToken);

                if (!string.IsNullOrEmpty(_settings.HealthCheckIntervalCronExpression))
                {
                    var expression = CronExpression.Parse(_settings.HealthCheckIntervalCronExpression);

                    var utcNow = DateTimeOffset.UtcNow;
                    var nextUtc = expression.GetNextOccurrence(utcNow, TimeZoneInfo.Utc);

                    while(!stoppingToken.IsCancellationRequested && nextUtc.HasValue)
                    {
                        await Task.Delay((nextUtc! - utcNow).Value);

                        try
                        {
                            await RunHealthCheckAndPublishHealthReport(stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, $"Error in {nameof(MyHealthCheckBackgroundService)}.");
                        }

                        utcNow = DateTimeOffset.UtcNow;
                        nextUtc = expression.GetNextOccurrence(utcNow, TimeZoneInfo.Utc);                        
                    }
                }
                else if (_settings.HealthCheckIntervalInMinutes.HasValue)
                {
                    TimeSpan interval = TimeSpan.FromMinutes(_settings.HealthCheckIntervalInMinutes.Value);

                    using PeriodicTimer timer = new PeriodicTimer(interval);

                    while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
                    {
                        try
                        {
                            await RunHealthCheckAndPublishHealthReport(stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, $"Error in {nameof(MyHealthCheckBackgroundService)}.");
                        }
                    }
                }
                else
                {
                    throw new ApplicationException("Please specify health check interval in cron expression or minutes");
                }                
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error in {nameof(MyHealthCheckBackgroundService)}.");
            }            
        }

        private async Task RunHealthCheckAndPublishHealthReport(CancellationToken stoppingToken)
        {
            var report = await _myHealthCheckService.CheckHealthAsync(stoppingToken);

            await _myHealthCheckService.PublishHealthReportAsync(report);
        }
    }
}