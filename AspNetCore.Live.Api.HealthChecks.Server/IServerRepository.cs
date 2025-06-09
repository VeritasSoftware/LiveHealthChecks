namespace AspNetCore.Live.Api.HealthChecks.Server
{
    public interface IServerRepository
    {
        Task AddHealthCheckAsync(MyHealthCheckModel model);
    }
}
