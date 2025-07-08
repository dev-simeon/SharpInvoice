namespace SharpInvoice.Shared.Infrastructure.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;

public enum EmailTemplate
{
    EmailConfirmation,
    PasswordReset,
    TwoFactorAuth,
    TeamInvitation,
    TeamMemberAdded
}

public interface IEmailTemplateRenderer
{
    Task<string> RenderAsync(EmailTemplate template, Dictionary<string, string> data);
} 