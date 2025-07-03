namespace SharpInvoice.Modules.Auth.Infrastructure.Services;

using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Configuration;
using System.Net;

public class SendGridEmailSender(IOptions<SendGridSettings> sendGridSettings) : IEmailSender
{
    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var client = new SendGridClient(sendGridSettings.Value.ApiKey);
        var from = new EmailAddress(sendGridSettings.Value.FromEmail, sendGridSettings.Value.FromName);
        var to = new EmailAddress(WebUtility.HtmlEncode(toEmail));
        
        // Sanitize subject to prevent header injection
        var safeSubject = WebUtility.HtmlEncode(subject);
        
        // The message content is assumed to be HTML and should be already sanitized by EmailTemplateRenderer
        var plainTextContent = message;
        var msg = MailHelper.CreateSingleEmail(from, to, safeSubject, plainTextContent, message);
        await client.SendEmailAsync(msg);
    }
}