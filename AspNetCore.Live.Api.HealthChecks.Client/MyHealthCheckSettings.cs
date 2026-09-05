using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspNetCore.Live.Api.HealthChecks.Client
{
    public class MyHealthCheckBasicSettings
    {
        public int? HealthCheckIntervalInMinutes { get; set; } = 15;
        public string? HealthCheckIntervalCronExpression { get; set; }
        public string HealthCheckServerHubUrl { get; set; } = string.Empty;        
        public bool PublishOnlyWhenNotHealthy { get; set; }
        public bool AddHealthCheckMiddleware { get; set; } = false;
    }
    public class MyHealthCheckSettings : MyHealthCheckBasicSettings
    {
        public string ClientId { get; set; } = string.Empty;
        public string ReceiveMethod { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public Func<HealthReport, object>? TransformHealthReport { get; set; } = null;        
    }

    // Wrapper that can swap the inner service
    public class MyHealthCheckSettingsHolder
    {
        private MyHealthCheckSettings _current;
        private readonly object _lock = new();

        public event Func<MyHealthCheckSettings, Task>? OnSettingsChanged;

        public MyHealthCheckSettingsHolder(MyHealthCheckSettings initial)
        {
            _current = initial;
        }

        public MyHealthCheckSettings Current
        {
            get { lock (_lock) return _current; }
        }

        public void Replace(MyHealthCheckBasicSettings newBasicSettings)
        {
            if (newBasicSettings == null) throw new ArgumentNullException(nameof(newBasicSettings));
            lock (_lock)
            {
                _current.HealthCheckIntervalCronExpression = newBasicSettings.HealthCheckIntervalCronExpression;
                _current.HealthCheckIntervalInMinutes = newBasicSettings.HealthCheckIntervalInMinutes;
                _current.HealthCheckServerHubUrl = newBasicSettings.HealthCheckServerHubUrl;
                _current.PublishOnlyWhenNotHealthy = newBasicSettings.PublishOnlyWhenNotHealthy;
                _current.AddHealthCheckMiddleware = newBasicSettings.AddHealthCheckMiddleware;

                OnSettingsChanged?.Invoke(_current);
            }
        }
    }
}
