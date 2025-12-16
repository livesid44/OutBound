using SiebelLeadBackup.Console.Models;

namespace SiebelLeadBackup.Console.Services;

public interface ISiebelApiService
{
    Task<(bool Success, string Message)> SendLeadDataAsync(LeadData lead);
}
