using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SiebelLeadBackup.Console.Models;

namespace SiebelLeadBackup.Console.Services;

public class SiebelApiService : ISiebelApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SiebelApiService> _logger;
    private readonly string _authorization;

    public SiebelApiService(HttpClient httpClient, SiebelApiSettings settings, ILogger<SiebelApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _authorization = settings.Authorization;
        
        _httpClient.BaseAddress = new Uri(settings.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _authorization.Replace("Basic ", ""));
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<(bool Success, string Message)> SendLeadDataAsync(LeadData lead)
    {
        try
        {
            var requestBody = new SiebelRequestBody
            {
                Body = new SiebelBodyContent
                {
                    Freetext1 = lead.Column1,
                    Freetext2 = lead.Column2,
                    Freetext3 = lead.Column3,
                    Freetext4 = lead.Column4,
                    Freetext5 = lead.Column5,
                    Freetext6 = lead.Column6,
                    Freetext7 = lead.Column7,
                    Freetext8 = lead.Column8,
                    Freetext9 = lead.Column9
                }
            };

            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogDebug("Sending request for InteractionId: {InteractionId}", lead.Interactionid);

            var response = await _httpClient.PostAsync("", content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully sent lead data for InteractionId: {InteractionId}", lead.Interactionid);
                return (true, responseContent);
            }
            else
            {
                _logger.LogWarning("Failed to send lead data for InteractionId: {InteractionId}. Status: {StatusCode}, Response: {Response}", 
                    lead.Interactionid, response.StatusCode, responseContent);
                return (false, $"HTTP {response.StatusCode}: {responseContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending lead data for InteractionId: {InteractionId}", lead.Interactionid);
            return (false, $"Exception: {ex.Message}");
        }
    }
}
