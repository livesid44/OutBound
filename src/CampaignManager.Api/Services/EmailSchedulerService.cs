using System.Text.Json;
using CampaignManager.Data.Repositories;
using CampaignManager.Shared.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CampaignManager.Api.Services;

public class EmailSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailSchedulerService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);

    public EmailSchedulerService(IServiceProvider serviceProvider, ILogger<EmailSchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Scheduler Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessEmailQueueAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email queue");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Email Scheduler Service stopped.");
    }

    private async Task ProcessEmailQueueAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Get all active campaigns with email channel
        var campaigns = await unitOfWork.Repository<Campaign>()
            .FindAsync(c => c.Status == CampaignStatus.Active && 
                           c.IsSchedulerEnabled &&
                           (c.Channels & ChannelType.Email) == ChannelType.Email);

        foreach (var campaign in campaigns)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                await ProcessCampaignEmailsAsync(unitOfWork, campaign, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing emails for campaign {CampaignId}", campaign.Id);
            }
        }
    }

    private async Task ProcessCampaignEmailsAsync(IUnitOfWork unitOfWork, Campaign campaign, CancellationToken stoppingToken)
    {
        // Get API config for this campaign
        var apiConfig = await unitOfWork.Repository<CampaignApiConfig>()
            .FirstOrDefaultAsync(c => c.CampaignId == campaign.Id);

        if (apiConfig == null || string.IsNullOrEmpty(apiConfig.SendGridApiKey))
        {
            _logger.LogWarning("Campaign {CampaignId} has no SendGrid API key configured", campaign.Id);
            return;
        }

        // Get strategy for retry limits
        var strategy = await unitOfWork.Repository<CampaignStrategy>()
            .FirstOrDefaultAsync(s => s.CampaignId == campaign.Id);

        int maxAttempts = strategy?.MaxEmailAttempts ?? 3;

        // Get leads pending email (not yet sent or failed with retries remaining)
        var pendingLeads = await unitOfWork.Repository<CampaignLead>()
            .FindAsync(l => l.CampaignId == campaign.Id &&
                           l.EmailStatus == LeadEmailStatus.Pending &&
                           l.EmailAttempts < maxAttempts);

        var leadsList = pendingLeads.Take(50).ToList(); // Process 50 at a time

        if (!leadsList.Any())
        {
            return;
        }

        _logger.LogInformation("Processing {Count} emails for campaign {CampaignId}", leadsList.Count, campaign.Id);

        // Get campaign fields for template substitution
        var fields = await unitOfWork.Repository<CampaignField>()
            .FindAsync(f => f.CampaignId == campaign.Id);
        var fieldList = fields.ToList();

        var client = new SendGridClient(apiConfig.SendGridApiKey);

        foreach (var lead in leadsList)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                var success = await SendEmailAsync(client, apiConfig, lead, fieldList);

                lead.EmailAttempts++;
                lead.LastEmailAttemptAt = DateTime.UtcNow;

                if (success)
                {
                    lead.EmailStatus = LeadEmailStatus.Sent;
                    lead.EmailSentAt = DateTime.UtcNow;
                    _logger.LogInformation("Email sent successfully for lead {LeadId}", lead.Id);
                }
                else
                {
                    if (lead.EmailAttempts >= maxAttempts)
                    {
                        lead.EmailStatus = LeadEmailStatus.Failed;
                    }
                    _logger.LogWarning("Email failed for lead {LeadId}, attempt {Attempt}/{MaxAttempts}", 
                        lead.Id, lead.EmailAttempts, maxAttempts);
                }

                unitOfWork.Repository<CampaignLead>().Update(lead);
                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception sending email for lead {LeadId}", lead.Id);
                lead.EmailAttempts++;
                lead.LastEmailAttemptAt = DateTime.UtcNow;
                if (lead.EmailAttempts >= maxAttempts)
                {
                    lead.EmailStatus = LeadEmailStatus.Failed;
                }
                unitOfWork.Repository<CampaignLead>().Update(lead);
                await unitOfWork.SaveChangesAsync();
            }

            // Small delay between emails to avoid rate limiting
            await Task.Delay(100, stoppingToken);
        }

        // Update campaign last processed time
        campaign.LastProcessedAt = DateTime.UtcNow;
        unitOfWork.Repository<Campaign>().Update(campaign);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task<bool> SendEmailAsync(SendGridClient client, CampaignApiConfig config, 
        CampaignLead lead, List<CampaignField> fields)
    {
        // Parse lead data
        var leadData = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(lead.Data))
        {
            try
            {
                leadData = JsonSerializer.Deserialize<Dictionary<string, string>>(lead.Data) 
                    ?? new Dictionary<string, string>();
            }
            catch
            {
                leadData = new Dictionary<string, string>();
            }
        }

        // Get email address from lead data
        string? toEmail = null;
        foreach (var field in fields.Where(f => f.FieldType == FieldType.Email))
        {
            if (leadData.TryGetValue(field.FieldName, out var email) && !string.IsNullOrEmpty(email))
            {
                toEmail = email;
                break;
            }
        }

        if (string.IsNullOrEmpty(toEmail))
        {
            _logger.LogWarning("No email address found for lead {LeadId}", lead.Id);
            return false;
        }

        // Prepare email content
        var subject = SubstituteFields(config.EmailSubject ?? "Campaign Email", leadData);
        var htmlContent = SubstituteFields(config.EmailTemplateHtml ?? "<p>Hello!</p>", leadData);

        // Get recipient name if available
        string toName = "";
        if (leadData.TryGetValue("FirstName", out var firstName))
        {
            toName = firstName;
            if (leadData.TryGetValue("LastName", out var lastName))
            {
                toName += " " + lastName;
            }
        }

        var from = new EmailAddress(
            config.SendGridFromEmail ?? "noreply@example.com",
            config.SendGridFromName ?? "Campaign Manager");
        var to = new EmailAddress(toEmail, toName);

        var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);

        var response = await client.SendEmailAsync(msg);

        return response.IsSuccessStatusCode;
    }

    private string SubstituteFields(string template, Dictionary<string, string> data)
    {
        var result = template;
        foreach (var kvp in data)
        {
            result = result.Replace("{{" + kvp.Key + "}}", kvp.Value ?? "");
        }
        return result;
    }
}
