namespace CustomCodeFramework.Messaging.Outbox.Locks;

public static class OutboxWorkerLockKeys
{
    public const string OutboxProcessor = "workers:messaging:outbox-processor";

    public const string InboxCleanup = "workers:messaging:inbox-cleanup";

    public static string ForServiceOutbox(string serviceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        return $"workers:{serviceName}:messaging:outbox-processor";
    }

    public static string ForServiceInboxCleanup(string serviceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        return $"workers:{serviceName}:messaging:inbox-cleanup";
    }
}
