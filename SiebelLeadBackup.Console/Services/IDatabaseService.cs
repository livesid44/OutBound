using SiebelLeadBackup.Console.Models;

namespace SiebelLeadBackup.Console.Services;

public interface IDatabaseService
{
    Task<List<LeadData>> GetPendingLeadsAsync();
    Task UpdateLeadStatusAsync(int interactionId, string message, string status);
}
