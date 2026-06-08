using MongoDB.Driver;

namespace CustomCodeFramework.Mongo.Abstractions;

public interface IMongoContext
{
    IMongoDatabase Database { get; }

    IMongoClient Client { get; }

    IMongoCollection<TDocument> GetCollection<TDocument>();

    IMongoCollection<TDocument> GetCollection<TDocument>(string collectionName);
}
