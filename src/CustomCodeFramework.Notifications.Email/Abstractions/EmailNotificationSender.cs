using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Email.Messages;
using CustomCodeFramework.Notifications.Email.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CustomCodeFramework.Notifications.Email.Abstractions;

public sealed class EmailNotificationSender(IOptions<EmailNotificationOptions> options)
    : IEmailNotificationSender
{
    public async Task<NotificationChannelResult> SendAsync(
        EmailNotificationMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        try
        {
            var emailOptions = options.Value;
            var mimeMessage = BuildMimeMessage(message, emailOptions);

            using var smtpClient = new SmtpClient { Timeout = emailOptions.TimeoutSeconds * 1000 };

            await smtpClient.ConnectAsync(
                emailOptions.Host,
                emailOptions.Port,
                ResolveSecureSocketOptions(emailOptions),
                cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(emailOptions.UserName))
            {
                await smtpClient.AuthenticateAsync(
                    emailOptions.UserName,
                    emailOptions.Password,
                    cancellationToken
                );
            }

            var providerMessageId = await smtpClient.SendAsync(mimeMessage, cancellationToken);

            await smtpClient.DisconnectAsync(quit: true, cancellationToken);

            return NotificationChannelResult.Success(providerMessageId);
        }
        catch (Exception exception)
        {
            return NotificationChannelResult.Failed(
                "email.send_failed",
                exception.Message,
                exception
            );
        }
    }

    private static MimeMessage BuildMimeMessage(
        EmailNotificationMessage message,
        EmailNotificationOptions options
    )
    {
        var mimeMessage = new MimeMessage();

        var from =
            message.From
            ?? new EmailAddress { Address = options.FromEmail, Name = options.FromName };

        mimeMessage.From.Add(ToMailboxAddress(from));
        mimeMessage.To.Add(ToMailboxAddress(message.To));

        foreach (var cc in message.Cc)
        {
            mimeMessage.Cc.Add(ToMailboxAddress(cc));
        }

        foreach (var bcc in message.Bcc)
        {
            mimeMessage.Bcc.Add(ToMailboxAddress(bcc));
        }

        mimeMessage.Subject = message.Subject;

        var bodyBuilder = new BodyBuilder();

        if (message.IsHtml)
        {
            bodyBuilder.HtmlBody = message.Body;
        }
        else
        {
            bodyBuilder.TextBody = message.Body;
        }

        foreach (var attachment in message.Attachments)
        {
            bodyBuilder.Attachments.Add(
                attachment.FileName,
                attachment.Content,
                ContentType.Parse(attachment.ContentType)
            );
        }

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        return mimeMessage;
    }

    private static MailboxAddress ToMailboxAddress(EmailAddress address)
    {
        return string.IsNullOrWhiteSpace(address.Name)
            ? MailboxAddress.Parse(address.Address)
            : new MailboxAddress(address.Name, address.Address);
    }

    private static SecureSocketOptions ResolveSecureSocketOptions(EmailNotificationOptions options)
    {
        if (options.UseSsl)
        {
            return SecureSocketOptions.SslOnConnect;
        }

        if (options.UseStartTls)
        {
            return SecureSocketOptions.StartTls;
        }

        return SecureSocketOptions.None;
    }
}
