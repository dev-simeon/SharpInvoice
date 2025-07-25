using Microsoft.Extensions.Options;
using System.Text.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using SharpInvoice.Core.Interfaces.Services;
using SharpInvoice.Infrastructure.Options;

namespace SharpInvoice.Infrastructure.Services;

public class EmailService(IOptions<AppSettings> appSettings) : IEmailService
{
    private readonly AppSettings _appSettings = appSettings.Value;

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var client = new SendGridClient(_appSettings.SendGridKey);
        var from = new EmailAddress("no-reply@sharpinvoice.com", "SharpInvoice");
        var toEmail = new EmailAddress(to);
        var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, body, body);
        await client.SendEmailAsync(msg, cancellationToken);
    }

    public async Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachment, string attachmentName, CancellationToken cancellationToken = default)
    {
        var client = new SendGridClient(_appSettings.SendGridKey);
        var from = new EmailAddress("no-reply@sharpinvoice.com", "SharpInvoice");
        var toEmail = new EmailAddress(to);
        var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, body, body);
        var base64Attachment = Convert.ToBase64String(attachment);
        msg.AddAttachment(attachmentName, base64Attachment);
        await client.SendEmailAsync(msg, cancellationToken);
    }

    public async Task SendEmailWithTemplateAsync(string to, string templateId, object templateData, CancellationToken cancellationToken = default)
    {
        var client = new SendGridClient(_appSettings.SendGridKey);
        var from = new EmailAddress("no-reply@sharpinvoice.com", "SharpInvoice");
        var toEmail = new EmailAddress(to);
        
        var msg = MailHelper.CreateSingleTemplateEmail(from, toEmail, templateId, templateData);
        
        // Add tracking settings
        msg.TrackingSettings = new TrackingSettings
        {
            ClickTracking = new ClickTracking { Enable = true },
            OpenTracking = new OpenTracking { Enable = true }
        };
        
        await client.SendEmailAsync(msg, cancellationToken);
    }

    public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
    {
        var subject = "Confirm Your SharpInvoice Account";
        var body = $@"
            <html>
            <body>
                <h2>Welcome to SharpInvoice!</h2>
                <p>Please confirm your email address by clicking the link below:</p>
                <p><a href='{confirmationLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Confirm Email</a></p>
                <p>If you did not create this account, please ignore this email.</p>
                <br/>
                <p>Best regards,<br/>The SharpInvoice Team</p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetAsync(string email, string resetLink)
    {
        var subject = "Reset Your SharpInvoice Password";
        var body = $@"
            <html>
            <body>
                <h2>Password Reset Request</h2>
                <p>We received a request to reset your password for your SharpInvoice account.</p>
                <p>Click the link below to reset your password:</p>
                <p><a href='{resetLink}' style='background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Reset Password</a></p>
                <p><strong>This link will expire in 24 hours.</strong></p>
                <p>If you did not request a password reset, please ignore this email or contact support if you have concerns.</p>
                <br/>
                <p>Best regards,<br/>The SharpInvoice Team</p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendInvitationAsync(string email, string businessName, string role, string invitationLink)
    {
        var subject = $"You've been invited to join {businessName} on SharpInvoice";
        var body = $@"
            <html>
            <body>
                <h2>Team Invitation</h2>
                <p>You have been invited to join <strong>{businessName}</strong> as a <strong>{role}</strong> on SharpInvoice.</p>
                <p>Click the link below to accept the invitation:</p>
                <p><a href='{invitationLink}' style='background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Accept Invitation</a></p>
                <p>This invitation will expire in 7 days.</p>
                <p>If you're not sure why you received this invitation, please contact the person who invited you.</p>
                <br/>
                <p>Best regards,<br/>The SharpInvoice Team</p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendInvoiceAsync(string email, string invoiceNumber, string businessName, decimal amount, string currency, string invoiceLink)
    {
        var subject = $"Invoice {invoiceNumber} from {businessName}";
        var body = $@"
            <html>
            <body>
                <h2>New Invoice from {businessName}</h2>
                <p>You have received a new invoice:</p>
                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 4px; margin: 20px 0;'>
                    <p><strong>Invoice Number:</strong> {invoiceNumber}</p>
                    <p><strong>Amount:</strong> {amount:C} {currency}</p>
                    <p><strong>From:</strong> {businessName}</p>
                </div>
                <p>Click the link below to view and pay your invoice:</p>
                <p><a href='{invoiceLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>View Invoice</a></p>
                <p>Thank you for your business!</p>
                <br/>
                <p>Best regards,<br/>{businessName}</p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendTwoFactorCodeAsync(string to, string code, CancellationToken cancellationToken = default)
    {
        var subject = "Your SharpInvoice Security Code";
        var body = $@"
            <html>
            <body>
                <h2>Security Verification Code</h2>
                <p>Your verification code is:</p>
                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 4px; margin: 20px 0; text-align: center;'>
                    <h1 style='font-family: monospace; letter-spacing: 3px; color: #007bff;'>{code}</h1>
                </div>
                <p>This code will expire in 10 minutes.</p>
                <p>If you did not request this code, please contact support immediately.</p>
                <br/>
                <p>Best regards,<br/>The SharpInvoice Team</p>
            </body>
            </html>";

        await SendEmailAsync(to, subject, body, cancellationToken);
    }
} 