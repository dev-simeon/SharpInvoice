namespace SharpInvoice.Modules.Auth.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Configuration;
using System.Net;
using System.Net.Mail;

public class SendGridEmailSender : IEmailSender
{
    private readonly SendGridSettings _settings;
    private readonly ILogger<SendGridEmailSender> _logger;

    public SendGridEmailSender(AppSettings appSettings, ILogger<SendGridEmailSender> logger)
    {
        _settings = appSettings.SendGrid;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        _logger.LogInformation("Sending email to {To} with subject '{Subject}' via SendGrid SMTP.", toEmail, subject);

        try
        {
            using var smtpClient = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.ApiKey)
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
            
            mailMessage.To.Add(toEmail);
            
            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Successfully sent email via SMTP to {Recipient}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via SendGrid SMTP to {Recipient}", toEmail);
            throw;
        }
    }
}