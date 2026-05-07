using System.Reflection;
using be.Domain.Attributes;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace be.Infrastructure.Database;

public class ReadContext
{
    private readonly IMongoDatabase _db;

    public ReadContext(IConfiguration config)
    {
        var connectionString = config.GetConnectionString("ReadConnectString")
                               ?? throw new InvalidOperationException("ConnectionStrings:ReadConnectString is missing");

        var client = new MongoClient(connectionString);
        var databaseName = MongoUrl.Create(connectionString).DatabaseName;

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new InvalidOperationException(
                "ReadConnectString must include database name, for example mongodb://host:27017/mini4rum_read");

        _db = client.GetDatabase(databaseName);
    }

    public IMongoCollection<TDocument> Collection<TDocument>()
    {
        var collectionAttribute = typeof(TDocument).GetCustomAttribute<BsonCollectionAttribute.BsonCollection>();

        if (collectionAttribute is null || string.IsNullOrWhiteSpace(collectionAttribute.CollectionName))
            throw new InvalidOperationException($"{typeof(TDocument).Name} is missing BsonCollection attribute");

        return _db.GetCollection<TDocument>(collectionAttribute.CollectionName);
    }
}