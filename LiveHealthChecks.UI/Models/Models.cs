using Microsoft.AspNetCore.SignalR.Client;

namespace LiveHealthChecks.UI.Models
{
    public class MyServerConnection
    {
        public HubConnection? Connection { get; set; }
    }

    public class MyHealthCheckAuthModel
    {
        public string ClientId { get; set; } = string.Empty;
        public string ReceiveMethod { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
    }

    public class MyApiHealthCheckModel
    {
        public string ApiName { get; set; } = string.Empty;
        public string ReceiveMethod { get; set; } = string.Empty;
    }

    public class DashboardSettings
    {
        public string ServerUrl { get; set; } = string.Empty;
        public string ServerReceiveMethod { get; set; } = string.Empty;
        public string ServerSecretKey { get; set; } = string.Empty;
        public string ServerClientId { get; set; } = string.Empty;
        public List<MyApiHealthCheckModel> Apis { get; set; } = new List<MyApiHealthCheckModel>();
    }

    public class HealthCheck
    {
        public string Api { get; set; } = string.Empty;
        public string ReceiveMethod { get; set; } = string.Empty;
        public DateTime? ReceiveTimeStamp { get; set; } = null;
        public HealthReport? Report { get; set; }
        public double Status { get; set; }
    }

    public struct HealthStatus
    {
        public const double Unhealthy = 1.00;

        public const double Healthy = 2.00;
    }

    public class HealthReport
    {
        public Status Status { get; set; }
        public string? TotalDuration { get; set; }
        public Dictionary<string, HealthReportEntry>? Entries { get; set; }
    }

    public struct HealthReportEntry
    {
        private static readonly IReadOnlyDictionary<string, object> _emptyReadOnlyDictionary = new Dictionary<string, object>();

        /// <summary>
        /// Creates a new <see cref="HealthReportEntry"/> with the specified values for <paramref name="status"/>, <paramref name="exception"/>,
        /// <paramref name="description"/>, and <paramref name="data"/>.
        /// </summary>
        /// <param name="status">A value indicating the health status of the component that was checked.</param>
        /// <param name="description">A human-readable description of the status of the component that was checked.</param>
        /// <param name="duration">A value indicating the health execution duration.</param>
        /// <param name="exception">An <see cref="Exception"/> representing the exception that was thrown when checking for status (if any).</param>
        /// <param name="data">Additional key-value pairs describing the health of the component.</param>
        public HealthReportEntry(HealthStatus status, string? description, TimeSpan duration, Exception? exception, IReadOnlyDictionary<string, object>? data)
            : this(status, description, duration, exception, data, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="HealthReportEntry"/> with the specified values for <paramref name="status"/>, <paramref name="exception"/>,
        /// <paramref name="description"/>, and <paramref name="data"/>.
        /// </summary>
        /// <param name="status">A value indicating the health status of the component that was checked.</param>
        /// <param name="description">A human-readable description of the status of the component that was checked.</param>
        /// <param name="duration">A value indicating the health execution duration.</param>
        /// <param name="exception">An <see cref="Exception"/> representing the exception that was thrown when checking for status (if any).</param>
        /// <param name="data">Additional key-value pairs describing the health of the component.</param>
        /// <param name="tags">Tags associated with the health check that generated the report entry.</param>
        public HealthReportEntry(HealthStatus status, string? description, TimeSpan duration, Exception? exception, IReadOnlyDictionary<string, object>? data, IEnumerable<string>? tags = null)
        {
            Status = status;
            Description = description;
            Duration = duration;
            Exception = exception;
            Data = data ?? _emptyReadOnlyDictionary;
            Tags = tags ?? Enumerable.Empty<string>();
        }


        /// <summary>
        /// Gets additional key-value pairs describing the health of the component.
        /// </summary>
        public IReadOnlyDictionary<string, object> Data { get; }

        /// <summary>
        /// Gets a human-readable description of the status of the component that was checked.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Gets the health check execution duration.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Gets an <see cref="System.Exception"/> representing the exception that was thrown when checking for status (if any).
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Gets the health status of the component that was checked.
        /// </summary>
        public HealthStatus Status { get; }

        /// <summary>
        /// Gets the tags associated with the health check.
        /// </summary>
        public IEnumerable<string> Tags { get; }
    }

    public enum Status
    {
        Unhealthy,
        Degraded,        
        Healthy
    }

    public class TransformedHealthReport
    {
        public string? Status { get; set; }
        public Dictionary<string, string>? Results { get; set; }
    }

    public class ApplicationLogs
    {
    }
}
