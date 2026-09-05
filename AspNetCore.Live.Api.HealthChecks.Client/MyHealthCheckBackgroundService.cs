using Cronos;

namespace AspNetCore.Live.Api.HealthChecks.Client
{
    public class MyHealthCheckBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private string? _previousCronExpression;
        private int? _previousHealthCheckInterval;
        private readonly MyHealthCheckSettingsHolder _settingsHolder;
        private readonly ILogger<MyHealthCheckBackgroundService>? _logger;
        private readonly IMyHealthCheckService _myHealthCheckService;

        private bool IsSettingsChanged { get; set; } = false;

        public MyHealthCheckBackgroundService(
                                                IServiceProvider serviceProvider,
                                                IMyHealthCheckService myHealthCheckService, 
                                                ILogger<MyHealthCheckBackgroundService>? logger = null
                                            )
        {            
            _serviceProvider = serviceProvider;
            _myHealthCheckService = myHealthCheckService;
            _logger = logger;

            _settingsHolder = _serviceProvider.GetRequiredService<MyHealthCheckSettingsHolder>();
            _previousCronExpression = _settingsHolder.Current.HealthCheckIntervalCronExpression;
            _previousHealthCheckInterval = _settingsHolder.Current.HealthCheckIntervalInMinutes;
            _settingsHolder.OnSettingsChanged += SettingsHolder_OnSettingsChanged;
        }

        private async Task SettingsHolder_OnSettingsChanged(MyHealthCheckSettings newSettings)
        {
            if (_previousCronExpression != newSettings.HealthCheckIntervalCronExpression ||
                _previousHealthCheckInterval != newSettings.HealthCheckIntervalInMinutes)
            {
                _logger?.LogInformation($"Health check settings changed. Restarting {nameof(MyHealthCheckBackgroundService)}.");

                _previousCronExpression = newSettings.HealthCheckIntervalCronExpression;
                _previousHealthCheckInterval = newSettings.HealthCheckIntervalInMinutes;
                IsSettingsChanged = true;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken = default)
        {
            try
            {
                await RunHealthCheckAndPublishHealthReport(stoppingToken);

                var settings = _settingsHolder.Current;

                if (!string.IsNullOrEmpty(settings.HealthCheckIntervalCronExpression))
                {
                    var expression = CronExpression.Parse(settings.HealthCheckIntervalCronExpression);

                    var utcNow = DateTimeOffset.UtcNow;
                    var nextUtc = expression.GetNextOccurrence(utcNow, TimeZoneInfo.Utc);

                    while(!stoppingToken.IsCancellationRequested && nextUtc.HasValue)
                    {
                        await Task.Delay((nextUtc! - utcNow).Value);

                        await RunHealthCheckAndPublishHealthReport(stoppingToken);

                        utcNow = DateTimeOffset.UtcNow;

                        if (IsSettingsChanged)
                        {
                            _logger?.LogInformation($"Health check settings changed. Restarting {nameof(MyHealthCheckBackgroundService)}.");
                            nextUtc = null;
                            expression = CronExpression.Parse(settings.HealthCheckIntervalCronExpression);
                            IsSettingsChanged = false;
                        }

                        nextUtc = expression.GetNextOccurrence(utcNow, TimeZoneInfo.Utc);                        
                    }

                    nextUtc = null;
                }
                else if (settings.HealthCheckIntervalInMinutes.HasValue)
                {
                    TimeSpan interval = TimeSpan.FromMinutes(settings.HealthCheckIntervalInMinutes.Value);

                    PeriodicTimer timer = new PeriodicTimer(interval);

                    while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
                    {                        
                        await RunHealthCheckAndPublishHealthReport(stoppingToken);

                        if (IsSettingsChanged)
                        {
                            _logger?.LogInformation($"Health check settings changed. Restarting {nameof(MyHealthCheckBackgroundService)}.");
                            interval = TimeSpan.FromMinutes(settings.HealthCheckIntervalInMinutes.Value);
                            timer = new PeriodicTimer(interval);
                            IsSettingsChanged = false;
                            break;
                        }
                    }

                    timer.Dispose();
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
            try
            {
                var report = await _myHealthCheckService.CheckHealthAsync(stoppingToken);

                await _myHealthCheckService.PublishHealthReportAsync(report);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error in {nameof(MyHealthCheckBackgroundService)}.");
            }            
        }
    }
}