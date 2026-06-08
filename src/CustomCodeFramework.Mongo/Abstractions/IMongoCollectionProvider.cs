using MongoDB.Driver;

namespace CustomCodeFramework.Mongo.Abstractions;

public interface IMongoCollectionProvider
{
    IMongoCollection<TDocument> GetCollection<TDocument>();

    IMongoCollection<TDocument> GetCollection<TDocument>(string collectionName);
}
