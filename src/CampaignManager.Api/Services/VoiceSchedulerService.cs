using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CampaignManager.Data.Repositories;
using CampaignManager.Shared.Models;

namespace CampaignManager.Api.Services;

public class VoiceSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VoiceSchedulerService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);

    public VoiceSchedulerService(IServiceProvider serviceProvider, ILogger<VoiceSchedulerService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Voice Scheduler Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessVoiceQueueAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing voice queue");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Voice Scheduler Service stopped.");
    }

    private async Task ProcessVoiceQueueAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Get all active campaigns with voice/call channel
        var campaigns = await unitOfWork.Repository<Campaign>()
            .FindAsync(c => c.Status == CampaignStatus.Active && 
                           c.IsSchedulerEnabled &&
                           (c.Channels & ChannelType.Call) == ChannelType.Call);

        foreach (var campaign in campaigns)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                await ProcessCampaignCallsAsync(unitOfWork, campaign, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing calls for campaign {CampaignId}", campaign.Id);
            }
        }
    }

    private async Task ProcessCampaignCallsAsync(IUnitOfWork unitOfWork, Campaign campaign, CancellationToken stoppingToken)
    {
        // Get API config for this campaign
        var apiConfig = await unitOfWork.Repository<CampaignApiConfig>()
            .FirstOrDefaultAsync(c => c.CampaignId == campaign.Id);

        if (apiConfig == null || string.IsNullOrEmpty(apiConfig.WebExApiEndpoint))
        {
            _logger.LogWarning("Campaign {CampaignId} has no WebEx API endpoint configured", campaign.Id);
            return;
        }

        // Get strategy for retry limits
        var strategy = await unitOfWork.Repository<CampaignStrategy>()
            .FirstOrDefaultAsync(s => s.CampaignId == campaign.Id);

        int maxAttempts = strategy?.MaxCallAttempts ?? 3;

        // Get leads pending call (not yet called or failed with retries remaining)
        var pendingLeads = await unitOfWork.Repository<CampaignLead>()
            .FindAsync(l => l.CampaignId == campaign.Id &&
                           l.CallStatus == LeadCallStatus.Pending &&
                           l.CallAttempts < maxAttempts);

        var leadsList = pendingLeads.Take(10).ToList(); // Process 10 at a time for calls

        if (!leadsList.Any())
        {
            return;
        }

        _logger.LogInformation("Processing {Count} calls for campaign {CampaignId}", leadsList.Count, campaign.Id);

        // Get campaign fields for parameter mapping
        var fields = await unitOfWork.Repository<CampaignField>()
            .FindAsync(f => f.CampaignId == campaign.Id);
        var fieldList = fields.ToList();

        foreach (var lead in leadsList)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                var success = await TriggerCallAsync(apiConfig, lead, fieldList);

                lead.CallAttempts++;
                lead.LastCallAttemptAt = DateTime.UtcNow;

                if (success)
                {
                    lead.CallStatus = LeadCallStatus.Dialed;
                    _logger.LogInformation("Call triggered successfully for lead {LeadId}", lead.Id);
                }
                else
                {
                    if (lead.CallAttempts >= maxAttempts)
                    {
                        lead.CallStatus = LeadCallStatus.Failed;
                    }
                    _logger.LogWarning("Call trigger failed for lead {LeadId}, attempt {Attempt}/{MaxAttempts}", 
                        lead.Id, lead.CallAttempts, maxAttempts);
                }

                unitOfWork.Repository<CampaignLead>().Update(lead);
                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception triggering call for lead {LeadId}", lead.Id);
                lead.CallAttempts++;
                lead.LastCallAttemptAt = DateTime.UtcNow;
                if (lead.CallAttempts >= maxAttempts)
                {
                    lead.CallStatus = LeadCallStatus.Failed;
                }
                unitOfWork.Repository<CampaignLead>().Update(lead);
                await unitOfWork.SaveChangesAsync();
            }

            // Delay between calls to manage pacing
            await Task.Delay(1000, stoppingToken);
        }

        // Update campaign last processed time
        campaign.LastProcessedAt = DateTime.UtcNow;
        unitOfWork.Repository<Campaign>().Update(campaign);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task<bool> TriggerCallAsync(CampaignApiConfig config, CampaignLead lead, List<CampaignField> fields)
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

        // Build request body from parameter mappings
        var requestBody = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(config.ParameterMappings))
        {
            try
            {
                var mappings = JsonSerializer.Deserialize<Dictionary<string, string>>(config.ParameterMappings)
                    ?? new Dictionary<string, string>();

                foreach (var mapping in mappings)
                {
                    var value = SubstituteFields(mapping.Value, leadData);
                    requestBody[mapping.Key] = value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing parameter mappings for campaign");
            }
        }

        // Add lead ID for tracking
        requestBody["lead_id"] = lead.Id.ToString();
        requestBody["campaign_id"] = lead.CampaignId.ToString();

        // Make the API call
        var client = _httpClientFactory.CreateClient();

        // Set auth token if provided
        if (!string.IsNullOrEmpty(config.WebExAuthToken))
        {
            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", config.WebExAuthToken);
        }

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(config.WebExApiEndpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("WebEx API call successful for lead {LeadId}", lead.Id);
                return true;
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("WebEx API call failed with status {StatusCode}: {Response}", 
                    response.StatusCode, responseContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception calling WebEx API for lead {LeadId}", lead.Id);
            return false;
        }
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
