using Blazored.LocalStorage;
using LiveHealthChecks.UI.Models;

namespace LiveHealthChecks.UI.Repository
{
    public interface IMyHealthChecksRepository
    {
        ValueTask<bool> ContainKeyAsync(string key, CancellationToken cancellationToken = default);
        ValueTask<List<HealthCheck>> GetHealthChecksDataAsync(string receiveMethod, CancellationToken cancellationToken = default);
        ValueTask SetHealthChecksDataAsync(string receiveMethod, List<HealthCheck> data, CancellationToken cancellationToken = default);
    }


    public class MyHealthChecksRepository: IMyHealthChecksRepository
    {
        private readonly ILocalStorageService _localStorageService;

        public MyHealthChecksRepository(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }

        public async ValueTask<bool> ContainKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            return await _localStorageService.ContainKeyAsync(key, cancellationToken);
        }

        public async ValueTask<List<HealthCheck>> GetHealthChecksDataAsync(string receiveMethod, CancellationToken cancellationToken = default)
        {
            return await _localStorageService.GetItemAsync<List<HealthCheck>>(receiveMethod, cancellationToken);
        }

        public async ValueTask SetHealthChecksDataAsync(string receiveMethod, List<HealthCheck> data, CancellationToken cancellationToken = default)
        {
            await _localStorageService.SetItemAsync(receiveMethod, data, cancellationToken);
        }
    }
}
