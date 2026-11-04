namespace Portfolio.Domain.Notifications;

/// <summary>
/// Defines the contract for sending emails.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email message asynchronously.
    /// </summary>
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}