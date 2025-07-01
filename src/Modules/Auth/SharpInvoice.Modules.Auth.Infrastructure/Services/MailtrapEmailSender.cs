namespace SharpInvoice.Modules.Auth.Infrastructure.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SharpInvoice.Shared.Infrastructure.Configuration;

public class MailtrapEmailSender : IEmailSender
{
    private readonly ILogger<MailtrapEmailSender> _logger;
    private readonly HttpClient _httpClient;
    private readonly string? _inboxId;
    private readonly MailtrapSettings _settings;

    public MailtrapEmailSender(
        HttpClient httpClient,
        ILogger<MailtrapEmailSender> logger,
        IOptions<MailtrapSettings> mailtrapSettings)
    {
        _logger = logger;
        _httpClient = httpClient;
        _settings = mailtrapSettings.Value;
        _inboxId = _settings.InboxId;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlContent)
    {
        _logger.LogInformation("Sending email to {To} with subject '{Subject}' via Mailtrap.", to, subject);

        var requestBody = new
        {
            to = new[] { new { email = to } },
            subject,
            html = htmlContent
        };

        var jsonBody = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        try
        {
            if (string.IsNullOrWhiteSpace(_inboxId))
            {
                _logger.LogError("Mailtrap inbox ID is not set in configuration.");
                return;
            }
            var response = await _httpClient.PostAsync($"api/send/{_inboxId}", content);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully sent email to {To} via Mailtrap.", to);
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send email via Mailtrap. Status: {StatusCode}, Response: {Response}", response.StatusCode, responseContent);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to Mailtrap failed.");
        }
    }
}