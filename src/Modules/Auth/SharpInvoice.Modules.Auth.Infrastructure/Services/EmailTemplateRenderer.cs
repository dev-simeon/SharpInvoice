namespace SharpInvoice.Modules.Auth.Infrastructure.Services;

using SharpInvoice.Modules.Auth.Application.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

public class EmailTemplateRenderer : IEmailTemplateRenderer
{
    public async Task<string> RenderAsync(EmailTemplate template, Dictionary<string, string> data)
    {
        var templateFileName = template switch
        {
            EmailTemplate.EmailConfirmation => "EmailConfirmation.html",
            EmailTemplate.PasswordReset => "PasswordReset.html",
            _ => throw new KeyNotFoundException("The specified email template is not supported.")
        };

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"SharpInvoice.Modules.Auth.Infrastructure.Templates.{templateFileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException("The email template could not be found as an embedded resource.", resourceName);
        using var reader = new StreamReader(stream);
        var templateContent = await reader.ReadToEndAsync();

        foreach (var entry in data)
        {
            templateContent = templateContent.Replace($"{{{{{entry.Key}}}}}", entry.Value);
        }

        return templateContent;
    }
} 