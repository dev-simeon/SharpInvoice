using SharpInvoice.Core.Interfaces.Services;

namespace SharpInvoice.Infrastructure.Services;

public class NotificationService : INotificationService
{
    public string GenerateEmailConfirmationToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    public Task SendEmailConfirmationAsync(string email, string token)
    {
        // integrate with SendGrid/MailJet/etc.
        return Task.CompletedTask;
    }
}

