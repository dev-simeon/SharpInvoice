using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Configuration;

namespace SharpInvoice.Modules.Auth.Infrastructure.Services;

public class SendGridEmailSender(IOptions<SendGridSettings> sendGridSettings) : IEmailSender
{
    private readonly SendGridSettings _sendGridSettings = sendGridSettings.Value;

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var client = new SendGridClient(_sendGridSettings.ApiKey);
        var from = new EmailAddress(_sendGridSettings.FromEmail, _sendGridSettings.FromName);
        var to = new EmailAddress(toEmail);
        var plainTextContent = message;
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, message);
        await client.SendEmailAsync(msg);
    }
}