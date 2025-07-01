namespace SharpInvoice.Modules.Auth.Application.Interfaces;

public enum EmailTemplate
{
    EmailConfirmation,
    PasswordReset,
    TwoFactorAuth // Added for 2FA
}

public interface IEmailTemplateRenderer
{
    Task<string> RenderAsync(EmailTemplate template, Dictionary<string, string> data);
}