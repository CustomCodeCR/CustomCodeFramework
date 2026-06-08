using CustomCodeFramework.Core.Abstractions;

namespace CustomCodeFramework.Auth.Abstractions;

public interface ICurrentUserService : ICurrentUser
{
    string? TenantId { get; }

    string? SessionId { get; }
}
