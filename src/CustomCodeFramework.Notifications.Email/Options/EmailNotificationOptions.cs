namespace CustomCodeFramework.Notifications.Email.Options;

public sealed class EmailNotificationOptions
{
    public const string SectionName = "Notifications:Email";

    public string Host { get; init; } = string.Empty;

    public int Port { get; init; } = 587;

    public bool UseSsl { get; init; } = false;

    public bool UseStartTls { get; init; } = true;

    public string? UserName { get; init; }

    public string? Password { get; init; }

    public string FromEmail { get; init; } = string.Empty;

    public string FromName { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 30;
}
