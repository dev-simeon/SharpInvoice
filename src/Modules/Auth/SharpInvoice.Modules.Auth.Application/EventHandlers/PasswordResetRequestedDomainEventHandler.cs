namespace SharpInvoice.Modules.Auth.Application.EventHandlers;

using MediatR;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Modules.Auth.Domain.Entities;
using SharpInvoice.Modules.Auth.Domain.Events;
using SharpInvoice.Shared.Infrastructure.Configuration;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class PasswordResetRequestedDomainEventHandler(
    IUserRepository userRepository,
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IEmailSender emailSender,
    IEmailTemplateRenderer templateRenderer,
    IUnitOfWork unitOfWork,
    AppSettings appSettings) : INotificationHandler<PasswordResetRequestedDomainEvent>
{
    private readonly string _appUrl = appSettings.AppUrl;

    public async Task Handle(PasswordResetRequestedDomainEvent notification, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(notification.UserId);
        if (user == null) return;

        const int tokenValidForMinutes = 60;
        var token = PasswordResetToken.Create(notification.Token, user.Email, tokenValidForMinutes);
        await passwordResetTokenRepository.AddAsync(token);

        var resetLink = $"{_appUrl}/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(notification.Token)}";
        var templateData = new Dictionary<string, string> { { "name", user.FirstName }, { "link", resetLink } };
        var emailBody = await templateRenderer.RenderAsync(EmailTemplate.PasswordReset, templateData);
        await emailSender.SendEmailAsync(user.Email, "Reset Your SharpInvoice Password", emailBody);

        // The UnitOfWork will be saved by the original command handler
    }
}