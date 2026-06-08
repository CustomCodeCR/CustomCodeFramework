namespace CustomCodeFramework.Storage.Security;

public sealed record SignedUrlOptions
{
    public TimeSpan ExpiresIn { get; init; } = TimeSpan.FromMinutes(15);

    public FilePermission Permission { get; init; } = FilePermission.Read;
}
