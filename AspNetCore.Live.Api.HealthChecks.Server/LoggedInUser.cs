﻿namespace AspNetCore.Live.Api.HealthChecks.Server
{
    public class LoggedInUser
    {
        public string ClientId { get; set; } = string.Empty;
        public string ReceiveMethod { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
    }
}
