namespace CustomCodeFramework.Postgres.Abstractions;

public interface IPostgresDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
