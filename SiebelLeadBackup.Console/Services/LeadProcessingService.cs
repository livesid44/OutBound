using Microsoft.Extensions.Logging;
using SiebelLeadBackup.Console.Models;

namespace SiebelLeadBackup.Console.Services;

public class LeadProcessingService : ILeadProcessingService
{
    private readonly IDatabaseService _databaseService;
    private readonly ISiebelApiService _siebelApiService;
    private readonly ILogger<LeadProcessingService> _logger;
    private readonly int _maxDegreeOfParallelism;

    public LeadProcessingService(
        IDatabaseService databaseService,
        ISiebelApiService siebelApiService,
        ProcessingSettings settings,
        ILogger<LeadProcessingService> logger)
    {
        _databaseService = databaseService;
        _siebelApiService = siebelApiService;
        _logger = logger;
        _maxDegreeOfParallelism = settings.MaxDegreeOfParallelism;
    }

    public async Task ProcessLeadsAsync()
    {
        _logger.LogInformation("Starting lead processing...");

        try
        {
            var leads = await _databaseService.GetPendingLeadsAsync();

            if (leads.Count == 0)
            {
                _logger.LogInformation("No pending leads found.");
                return;
            }

            _logger.LogInformation("Processing {Count} leads with max parallelism of {Parallelism}", 
                leads.Count, _maxDegreeOfParallelism);

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = _maxDegreeOfParallelism
            };

            var successCount = 0;
            var failureCount = 0;

            await Parallel.ForEachAsync(leads, options, async (lead, cancellationToken) =>
            {
                try
                {
                    var (success, message) = await _siebelApiService.SendLeadDataAsync(lead);

                    var status = success ? "Success" : "Failed";

                    await _databaseService.UpdateLeadStatusAsync(lead.Interactionid, message, status);

                    if (success)
                    {
                        Interlocked.Increment(ref successCount);
                    }
                    else
                    {
                        Interlocked.Increment(ref failureCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing lead with InteractionId: {InteractionId}", lead.Interactionid);
                    Interlocked.Increment(ref failureCount);
                }
            });

            _logger.LogInformation("Lead processing completed. Success: {Success}, Failed: {Failed}", 
                successCount, failureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during lead processing");
            throw;
        }
    }
}
