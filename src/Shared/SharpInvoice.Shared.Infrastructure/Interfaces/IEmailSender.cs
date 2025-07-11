namespace SharpInvoice.Shared.Infrastructure.Interfaces;

using System.Threading.Tasks;

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string message);
} 