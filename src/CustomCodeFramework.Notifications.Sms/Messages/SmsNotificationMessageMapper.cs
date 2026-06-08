using CustomCodeFramework.Notifications.Messages;

namespace CustomCodeFramework.Notifications.Sms.Messages;

public static class SmsNotificationMessageMapper
{
    public static SmsNotificationMessage Map(NotificationMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrWhiteSpace(message.Recipient.PhoneNumber))
        {
            throw new InvalidOperationException("SMS recipient phone number is required.");
        }

        return new SmsNotificationMessage
        {
            MessageId = message.MessageId,
            To = new PhoneNumber { Value = message.Recipient.PhoneNumber },
            Body = message.Body,
            Data = message.Data,
            CreatedAtUtc = message.CreatedAtUtc,
        };
    }
}
