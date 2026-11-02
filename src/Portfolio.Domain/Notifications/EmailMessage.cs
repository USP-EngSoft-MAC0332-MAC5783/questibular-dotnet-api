namespace Portfolio.Domain.Notifications;

/// <summary>
/// Represents an email message to be sent to a user.
/// </summary>
/// <param name="To">The recipient's email address.</param>
/// <param name="Subject">The email subject.</param>
/// <param name="Body">The email body content.</param>
/// <param name="IsHtml">Indicates whether the body is HTML formatted.</param>
public sealed record EmailMessage(
    string To,
    string Subject,
    string Body,
    bool IsHtml = false);