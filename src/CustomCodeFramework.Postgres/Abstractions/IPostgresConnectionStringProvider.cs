namespace CustomCodeFramework.Postgres.Abstractions;

public interface IPostgresConnectionStringProvider
{
    string GetConnectionString();
}
