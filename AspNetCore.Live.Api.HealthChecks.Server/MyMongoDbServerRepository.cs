using MongoDB.Driver;

namespace AspNetCore.Live.Api.HealthChecks.Server
{
    public class MyMongoDbServerRepository : IServerRepository
    {
        private readonly IMongoDatabase _database;

        private static string _collectionName = string.Empty;

        public MyMongoDbServerRepository(IMongoClient client, MyHealthCheckSettings settings)
        {
            if (string.IsNullOrEmpty(_collectionName))
            {
                _collectionName = settings.DatabaseName;
            }

            _database = client.GetDatabase(_collectionName);
        }

        public async Task AddHealthCheckAsync(MyHealthCheckModel model)
        {
            var dbModel = new MyHealthCheckMongoDbModel
            {
                ClientId = model.ClientId,
                ReceiveMethod = model.ReceiveMethod,
                Report = model.Report,
                Timestamp = model.Timestamp
            };

            await AddHealthCheckAsync(dbModel);
        }

        private async Task AddHealthCheckAsync(MyHealthCheckMongoDbModel model)
        {
            var collection = _database.GetCollection<MyHealthCheckMongoDbModel>(model.ReceiveMethod);

            await collection.InsertOneAsync(model, new InsertOneOptions
            {
                BypassDocumentValidation = true
            });
        }
    }
}
