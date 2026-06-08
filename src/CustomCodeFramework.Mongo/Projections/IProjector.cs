namespace CustomCodeFramework.Mongo.Projections;

public interface IProjector<in TEvent>
{
    Task ProjectAsync(TEvent @event, CancellationToken cancellationToken = default);
}
