using Blazored.LocalStorage;
using LiveHealthChecks.UI.Models;

namespace LiveHealthChecks.UI.Repository
{
    public interface IMyHealthChecksRepository
    {
        ValueTask<bool> ContainKeyAsync(string key, CancellationToken cancellationToken = default);
        ValueTask<List<HealthCheck>> GetHealthChecksDataAsync(string receiveMethod, CancellationToken cancellationToken = default);
        ValueTask SetHealthChecksDataAsync(string receiveMethod, List<HealthCheck> data, CancellationToken cancellationToken = default);
        ValueTask DeleteHealthChecksAsync(int hours, params string[] receiveMethods);
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

        public async ValueTask DeleteHealthChecksAsync(int hours, params string[] receiveMethods)
        {
            if (hours == int.MaxValue)
            {
                receiveMethods.ToList().ForEach(async receiveMethod =>
                {
                    await _localStorageService.RemoveItemAsync(receiveMethod);
                });

                return;
            }

            var dateTime = DateTime.Now.AddHours(hours).ToUniversalTime();

            receiveMethods.ToList().ForEach(async receiveMethod =>
            {
                var healthChecks = await GetHealthChecksDataAsync(receiveMethod);

                healthChecks.RemoveAll(hc => hc.ReceiveTimeStamp > dateTime);

                await SetHealthChecksDataAsync(receiveMethod, healthChecks);
            });

            await Task.CompletedTask;
        }
    }
}
