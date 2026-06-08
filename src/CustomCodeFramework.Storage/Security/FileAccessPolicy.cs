namespace CustomCodeFramework.Storage.Security;

public sealed record FileAccessPolicy
{
    public FileVisibility Visibility { get; init; } = FileVisibility.Private;

    public IReadOnlyCollection<string> AllowedUserIds { get; init; } = [];

    public IReadOnlyCollection<string> AllowedRoles { get; init; } = [];

    public FilePermission Permissions { get; init; } = FilePermission.Read;
}
