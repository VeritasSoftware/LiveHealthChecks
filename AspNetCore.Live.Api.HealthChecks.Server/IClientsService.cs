namespace AspNetCore.Live.Api.HealthChecks.Server
{
    public interface IClientsService
    {
        Task<ClientSettings[]> GetClientsAsync();
    }
}
