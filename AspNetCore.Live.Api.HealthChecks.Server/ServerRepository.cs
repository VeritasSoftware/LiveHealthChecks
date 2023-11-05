using MongoDB.Driver;

namespace AspNetCore.Live.Api.HealthChecks.Server
{
    public interface IServerRepository
    {
        Task AddHealthCheckAsync(MyHealthCheckDbModel model);
    }

    public class ServerRepository : IServerRepository
    {
        private readonly IMongoDatabase _database;

        public ServerRepository(IMongoClient client)
        {
            _database = client.GetDatabase("ServerDb");
        }

        public async Task AddHealthCheckAsync(MyHealthCheckDbModel model)
        {
            var collection = _database.GetCollection<MyHealthCheckDbModel>(model.ReceiveMethod);

            await collection.InsertOneAsync(model, new InsertOneOptions
            {
                BypassDocumentValidation = true
            });
        }
    }
}
