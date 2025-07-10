using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using SharpInvoice.Core.Interfaces.Services;
using SharpInvoice.Infrastructure.Shared;

namespace SharpInvoice.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly SmtpClient _smtpClient;

    public EmailService(IOptions<AppSettings> appSettings)
    {
        _smtpSettings = appSettings.Value.Smtp;
        
        // Configure SMTP client for SendGrid
        _smtpClient = new SmtpClient
        {
            Host = _smtpSettings.Host,
            Port = _smtpSettings.Port,
            EnableSsl = _smtpSettings.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(
                _smtpSettings.Username,
                _smtpSettings.Password
            )
        };
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_smtpSettings.FromAddress, _smtpSettings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };
        
        mailMessage.To.Add(to);
        
        // Add SendGrid specific tracking settings via X-SMTPAPI header
        var smtpApiHeader = new
        {
            category = new[] { "SharpInvoice" },
            tracking_settings = new
            {
                click_tracking = new { enable = true },
                open_tracking = new { enable = true }
            }
        };
        
        string headerJson = JsonSerializer.Serialize(smtpApiHeader);
        mailMessage.Headers.Add("X-SMTPAPI", headerJson);
        
        await _smtpClient.SendMailAsync(mailMessage);
    }

    public async Task SendEmailWithTemplateAsync(string to, string templateId, object templateData)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_smtpSettings.FromAddress, _smtpSettings.FromName),
            Subject = "SharpInvoice Notification", // Subject will be replaced by template
            Body = "Please view this email with an email client that supports HTML content.",
            IsBodyHtml = true
        };
        
        mailMessage.To.Add(to);
        
        // Add SendGrid template ID and substitutions via X-SMTPAPI header
        var smtpApiHeader = new
        {
            template_id = templateId,
            substitutions = templateData,
            category = new[] { "SharpInvoice" },
            tracking_settings = new
            {
                click_tracking = new { enable = true },
                open_tracking = new { enable = true }
            }
        };
        
        string headerJson = JsonSerializer.Serialize(smtpApiHeader);
        mailMessage.Headers.Add("X-SMTPAPI", headerJson);
        
        await _smtpClient.SendMailAsync(mailMessage);
    }

    public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
    {
        // Check if we have a template ID configured
        var templateId = _smtpSettings.Templates.EmailConfirmation;
        
        if (!string.IsNullOrEmpty(templateId))
        {
            // Use template-based email
            var templateData = new
            {
                confirmation_link = confirmationLink
            };
            
            await SendEmailWithTemplateAsync(email, templateId, templateData);
            return;
        }
        
        // Fallback to standard email
        string subject = "Confirm your email address";
        string body = $@"
            <h2>Welcome to SharpInvoice!</h2>
            <p>Thank you for registering. Please confirm your email address by clicking the link below:</p>
            <p><a href='{confirmationLink}'>Confirm Email</a></p>
            <p>If you did not register for SharpInvoice, please ignore this email.</p>
            <p>This link will expire in 24 hours.</p>
        ";
        
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetAsync(string email, string resetLink)
    {
        // Check if we have a template ID configured
        var templateId = _smtpSettings.Templates.PasswordReset;
        
        if (!string.IsNullOrEmpty(templateId))
        {
            // Use template-based email
            var templateData = new
            {
                reset_link = resetLink
            };
            
            await SendEmailWithTemplateAsync(email, templateId, templateData);
            return;
        }
        
        // Fallback to standard email
        string subject = "Reset your password";
        string body = $@"
            <h2>Password Reset Request</h2>
            <p>We received a request to reset your password. Please click the link below to set a new password:</p>
            <p><a href='{resetLink}'>Reset Password</a></p>
            <p>If you did not request a password reset, please ignore this email.</p>
            <p>This link will expire in 30 minutes.</p>
        ";
        
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendInvitationAsync(string email, string businessName, string role, string invitationLink)
    {
        // Check if we have a template ID configured
        var templateId = _smtpSettings.Templates.Invitation;
        
        if (!string.IsNullOrEmpty(templateId))
        {
            // Use template-based email
            var templateData = new
            {
                business_name = businessName,
                role_name = role,
                invitation_link = invitationLink
            };
            
            await SendEmailWithTemplateAsync(email, templateId, templateData);
            return;
        }
        
        // Fallback to standard email
        string subject = $"You've been invited to join {businessName}";
        string body = $@"
            <h2>Business Invitation</h2>
            <p>You've been invited to join <strong>{businessName}</strong> as a <strong>{role}</strong>.</p>
            <p>Click the link below to accept this invitation:</p>
            <p><a href='{invitationLink}'>Accept Invitation</a></p>
            <p>If you don't have a SharpInvoice account yet, you'll be able to create one after clicking the link.</p>
            <p>This invitation will expire in 7 days.</p>
        ";
        
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendInvoiceAsync(string email, string invoiceNumber, string businessName, decimal amount, string currency, string invoiceLink)
    {
        // Check if we have a template ID configured
        var templateId = _smtpSettings.Templates.Invoice;
        
        if (!string.IsNullOrEmpty(templateId))
        {
            // Use template-based email
            var templateData = new
            {
                invoice_number = invoiceNumber,
                business_name = businessName,
                amount = amount.ToString("N2"),
                currency,
                invoice_link = invoiceLink
            };
            
            await SendEmailWithTemplateAsync(email, templateId, templateData);
            return;
        }
        
        // Fallback to standard email
        string subject = $"Invoice {invoiceNumber} from {businessName}";
        string body = $@"
            <h2>New Invoice</h2>
            <p><strong>{businessName}</strong> has sent you an invoice.</p>
            <p>
                <strong>Invoice Number:</strong> {invoiceNumber}<br>
                <strong>Amount:</strong> {amount.ToString("N2")} {currency}
            </p>
            <p>Click the link below to view and pay this invoice:</p>
            <p><a href='{invoiceLink}'>View Invoice</a></p>
            <p>Thank you for your business!</p>
        ";
        
        await SendEmailAsync(email, subject, body);
    }
} 