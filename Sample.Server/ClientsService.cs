using AspNetCore.Live.Api.HealthChecks.Server;

namespace Sample.Server
{
    public class ClientsService : IClientsService
    {
        public async Task<ClientSettings[]> GetClientsAsync()
        {
            //You can fetch the Clients from database (for eg.).
            //You can cache the Clients too.
            return await Task.FromResult(new ClientSettings[]
            {
                new ClientSettings
                {
                    ReceiveMethod = "SampleApiHealth",
                    SecretKey = "43bf0968-17e0-4d22-816a-6eaadd766692"
                },
                new ClientSettings
                {
                    ReceiveMethod = "SampleApi2Health",
                    SecretKey = "ae6f9a48-259b-4d03-9956-a2bf8838aaa4"
                },
                //Optional
                //Monitoring app connecting with ReceiveMethod *
                //will receive notifications for all ReceiveMethods in the system.
                new ClientSettings {
                    ReceiveMethod = "*",
                    SecretKey = "f22f3fd2-687d-48a1-aa2f-f2c9181364eb"
                }
            });
        }
    }
}
