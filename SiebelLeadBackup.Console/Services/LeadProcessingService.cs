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
        _logger.LogInformation("=== Starting lead processing ===");
        _logger.LogInformation("Timestamp: {Timestamp}", DateTime.Now);

        try
        {
            var leads = await _databaseService.GetPendingLeadsAsync();

            if (leads.Count == 0)
            {
                _logger.LogInformation("No pending leads found.");
                return;
            }

            var totalLeads = leads.Count;
            _logger.LogInformation("Retrieved {TotalCount} pending leads from database", totalLeads);
            _logger.LogInformation("Processing with max parallelism of {Parallelism}", _maxDegreeOfParallelism);
            _logger.LogInformation("Expected to process all {TotalCount} records", totalLeads);

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = _maxDegreeOfParallelism
            };

            var successCount = 0;
            var failureCount = 0;
            var processedCount = 0;

            var startTime = DateTime.Now;

            await Parallel.ForEachAsync(leads, options, async (lead, cancellationToken) =>
            {
                var currentProcessed = Interlocked.Increment(ref processedCount);
                
                try
                {
                    _logger.LogInformation("Processing lead {Current}/{Total} - InteractionId: {InteractionId}", 
                        currentProcessed, totalLeads, lead.Interactionid);

                    var (success, message) = await _siebelApiService.SendLeadDataAsync(lead);

                    var status = success ? "Success" : "Failed";

                    await _databaseService.UpdateLeadStatusAsync(lead.Interactionid, message, status);

                    if (success)
                    {
                        Interlocked.Increment(ref successCount);
                        _logger.LogInformation("Lead {InteractionId} processed successfully ({Current}/{Total})", 
                            lead.Interactionid, currentProcessed, totalLeads);
                    }
                    else
                    {
                        Interlocked.Increment(ref failureCount);
                        _logger.LogWarning("Lead {InteractionId} processing failed: {Message} ({Current}/{Total})", 
                            lead.Interactionid, message, currentProcessed, totalLeads);
                    }
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref failureCount);
                    _logger.LogError(ex, "Exception processing lead {InteractionId} ({Current}/{Total})", 
                        lead.Interactionid, currentProcessed, totalLeads);
                }
            });

            var endTime = DateTime.Now;
            var duration = endTime - startTime;

            _logger.LogInformation("=== Lead processing completed ===");
            _logger.LogInformation("Total leads retrieved: {Total}", totalLeads);
            _logger.LogInformation("Total leads processed: {Processed}", processedCount);
            _logger.LogInformation("Successfully processed: {Success}", successCount);
            _logger.LogInformation("Failed to process: {Failed}", failureCount);
            _logger.LogInformation("Processing duration: {Duration}", duration);
            _logger.LogInformation("Average time per lead: {AvgTime} ms", 
                processedCount > 0 ? duration.TotalMilliseconds / processedCount : 0);

            if (processedCount != totalLeads)
            {
                _logger.LogWarning("WARNING: Not all leads were processed! Expected {Total}, Processed {Processed}", 
                    totalLeads, processedCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during lead processing");
            throw;
        }
    }
}
