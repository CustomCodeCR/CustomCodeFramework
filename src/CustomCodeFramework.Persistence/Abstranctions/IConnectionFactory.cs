using System.Data.Common;

namespace CustomCodeFramework.Persistence.Abstractions;

public interface IConnectionFactory
{
    Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
