namespace SharpInvoice.Core.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendEmailConfirmationAsync(string email, string confirmationLink);
    Task SendPasswordResetAsync(string email, string resetLink);
    Task SendInvitationAsync(string email, string businessName, string role, string invitationLink);
    Task SendInvoiceAsync(string email, string invoiceNumber, string businessName, decimal amount, string currency, string invoiceLink);
} 