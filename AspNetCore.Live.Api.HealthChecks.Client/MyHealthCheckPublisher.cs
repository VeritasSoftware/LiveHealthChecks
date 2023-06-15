﻿using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace AspNetCore.Live.Api.HealthChecks.Client
{
    public interface IMyHealthCheckPublisher
    {
        Task Publish(HealthReport healthReport);
    }

    public class MyHealthCheckPublisher : IMyHealthCheckPublisher
    {
        private readonly MyHealthCheckSettings _settings;
        private readonly ILogger<MyHealthCheckPublisher>? _logger;

        public MyHealthCheckPublisher(MyHealthCheckSettings settings, ILogger<MyHealthCheckPublisher>? logger = null)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task Publish(HealthReport healthReport)
        {
            bool isTransform = _settings.TransformHealthReport != null;
            string? publishedReport = null;

            publishedReport = JsonSerializer.Serialize(isTransform 
                                                ? _settings.TransformHealthReport?.Invoke(healthReport)
                                                : healthReport);
            
            var connection = MyHealthCheckExtensions._healthChecksHubConnection;

            if (connection == null)
            {
                _logger?.LogError("The Server Hub connection is null.");
                return;
            }

            if (connection.State != HubConnectionState.Connected)
                await connection.StartAsync();

            _logger?.LogInformation($"Published Health Report: {publishedReport}");

            var request = new MyHealthCheckModel
            {
                ReceiveMethod = _settings.ReceiveMethod,
                SecretKey = _settings.SecretKey,
                Report = publishedReport
            };

            await connection.InvokeAsync("PublishMyHealthCheck", request);
        }
    }
}
