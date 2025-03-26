using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace AspNetCore.Live.Api.HealthChecks.Server
{
    public interface IServerRepository
    {
        Task AddHealthCheckAsync(MyHealthCheckDbModel model);
    }

    public class ServerRepository : IServerRepository
    {
        private readonly IMongoDatabase _database;

        private static string _collectionName = "ServerDb";

        public ServerRepository(IMongoClient client, MyHealthCheckSettings settings)
        {
            if (string.IsNullOrEmpty(_collectionName))
            {
                var m = Regex.Match(settings.DatabaseConnectionString!, @"^.*/(?<dbName>.+)$");

                _collectionName = m.Success ? m.Groups["dbName"].Captures[0].Value : "ServerDb";
            }

            _database = client.GetDatabase(_collectionName);
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
