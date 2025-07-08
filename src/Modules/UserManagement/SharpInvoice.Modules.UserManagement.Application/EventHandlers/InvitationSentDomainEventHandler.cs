namespace SharpInvoice.Modules.UserManagement.Application.EventHandlers;

using MediatR;
using SharpInvoice.Modules.UserManagement.Domain.Events;
using SharpInvoice.Shared.Infrastructure.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using System;
using SharpInvoice.Shared.Infrastructure.Interfaces;

public class InvitationSentDomainEventHandler(
    IInvitationRepository invitationRepository,
    IEmailSender emailSender,
    IEmailTemplateRenderer templateRenderer,
    AppSettings appSettings) : INotificationHandler<InvitationSentDomainEvent>
{
    public async Task Handle(InvitationSentDomainEvent notification, CancellationToken cancellationToken)
    {
        var invitation = await invitationRepository.GetByIdAsync(notification.InvitationId);
        if (invitation == null) return;

        var invitationLink = $"{appSettings.AppUrl}/accept-invitation?token={Uri.EscapeDataString(invitation.Token)}";
        var templateData = new Dictionary<string, string>
        {
            { "businessName", invitation.Business.Name },
            { "link", invitationLink }
        };

        var emailBody = await templateRenderer.RenderAsync(EmailTemplate.TeamInvitation, templateData);
        await emailSender.SendEmailAsync(invitation.InvitedUserEmail, $"You've been invited to join {invitation.Business.Name} on SharpInvoice", emailBody);
    }
}