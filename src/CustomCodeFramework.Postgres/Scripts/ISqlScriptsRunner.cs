namespace CustomCodeFramework.Postgres.Scripts;

public interface ISqlScriptRunner
{
    Task ExecuteAsync(string sql, CancellationToken cancellationToken = default);

    Task ExecuteFileAsync(string filePath, CancellationToken cancellationToken = default);
}
