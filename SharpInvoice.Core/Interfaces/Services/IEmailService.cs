namespace SharpInvoice.Core.Interfaces.Services;

/// <summary>
/// Interface for email services.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="body">The email body content.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email with attachment asynchronously.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="body">The email body content.</param>
    /// <param name="attachment">The attachment data.</param>
    /// <param name="attachmentName">The attachment file name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachment, string attachmentName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a two-factor authentication code via email.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="code">The 2FA verification code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendTwoFactorCodeAsync(string to, string code, CancellationToken cancellationToken = default);
} 