using CustomCodeFramework.Mongo.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Mongo.Indexes;

public sealed class MongoIndexRunner(IMongoContext mongoContext, IServiceProvider serviceProvider)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var builders = serviceProvider.GetServices<IMongoIndexBuilder>().ToArray();

        foreach (var builder in builders)
        {
            await builder.CreateIndexesAsync(mongoContext.Database, cancellationToken);
        }
    }
}
