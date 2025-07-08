namespace SharpInvoice.Modules.Auth.Application.EventHandlers;

using MediatR;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Modules.Auth.Domain.Events;
using SharpInvoice.Shared.Infrastructure.Configuration;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class UserCreatedDomainEventHandler(
    IUserRepository userRepository,
    IEmailSender emailSender,
    IEmailTemplateRenderer templateRenderer,
    AppSettings appSettings) : INotificationHandler<UserCreatedDomainEvent>
{
    private readonly string _appUrl = appSettings.AppUrl;

    public async Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(notification.UserId);
        if (user == null) return;

        var confirmationLink = $"{_appUrl}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(user.EmailConfirmationToken!)}";
        var templateData = new Dictionary<string, string> { { "name", user.FirstName }, { "link", confirmationLink } };
        var emailBody = await templateRenderer.RenderAsync(EmailTemplate.EmailConfirmation, templateData);
        await emailSender.SendEmailAsync(user.Email, "Confirm Your SharpInvoice Account", emailBody);
    }
}