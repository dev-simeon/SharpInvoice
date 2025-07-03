namespace SharpInvoice.Modules.Auth.Infrastructure.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SharpInvoice.Shared.Infrastructure.Configuration;

public class MailtrapEmailSender(
    HttpClient httpClient,
    ILogger<MailtrapEmailSender> logger,
    IOptions<MailtrapSettings> mailtrapSettings) : IEmailSender
{
    private readonly string? _inboxId = mailtrapSettings.Value.InboxId;

    public async Task SendEmailAsync(string to, string subject, string htmlContent)
    {
        logger.LogInformation("Sending email to {To} with subject '{Subject}' via Mailtrap.", to, subject);

        // Sanitize inputs - email is sanitized before putting in JSON
        var safeEmailAddress = WebUtility.HtmlEncode(to);
        var safeSubject = WebUtility.HtmlEncode(subject);
        
        // htmlContent should already be sanitized by EmailTemplateRenderer
        
        var requestBody = new
        {
            to = new[] { new { email = safeEmailAddress } },
            subject = safeSubject,
            html = htmlContent
        };

        var jsonBody = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        try
        {
            if (string.IsNullOrWhiteSpace(_inboxId))
            {
                logger.LogError("Mailtrap inbox ID is not set in configuration.");
                return;
            }
            var response = await httpClient.PostAsync($"api/send/{_inboxId}", content);
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Successfully sent email to {To} via Mailtrap.", to);
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                logger.LogError("Failed to send email via Mailtrap. Status: {StatusCode}, Response: {Response}", response.StatusCode, responseContent);
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP request to Mailtrap failed.");
        }
    }
}