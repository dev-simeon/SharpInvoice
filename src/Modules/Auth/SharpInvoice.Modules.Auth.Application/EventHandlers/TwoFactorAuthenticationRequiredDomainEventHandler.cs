namespace SharpInvoice.Modules.Auth.Application.EventHandlers;

using MediatR;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Modules.Auth.Domain.Events;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class TwoFactorAuthenticationRequiredDomainEventHandler(
    IUserRepository userRepository,
    IEmailSender emailSender,
    IEmailTemplateRenderer templateRenderer) : INotificationHandler<TwoFactorAuthenticationRequiredDomainEvent>
{
    public async Task Handle(TwoFactorAuthenticationRequiredDomainEvent notification, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(notification.UserId);
        if (user == null || !user.TwoFactorEnabled || user.TwoFactorCode == null) return;

        var templateData = new Dictionary<string, string> { { "name", user.FirstName }, { "code", user.TwoFactorCode } };
        var emailBody = await templateRenderer.RenderAsync(EmailTemplate.TwoFactorAuth, templateData);
        await emailSender.SendEmailAsync(user.Email, "Your SharpInvoice Verification Code", emailBody);
    }
}