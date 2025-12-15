using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SiebelLeadBackup.Console.Models;

namespace SiebelLeadBackup.Console.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(string connectionString, ILogger<DatabaseService> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<List<LeadData>> GetPendingLeadsAsync()
    {
        var leads = new List<LeadData>();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_getpendingsibiellead_backup", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                leads.Add(new LeadData
                {
                    Column1 = reader.GetString(0),
                    Column2 = reader.GetString(1),
                    Column3 = reader.GetString(2),
                    Column4 = reader.GetString(3),
                    Column5 = reader.GetString(4),
                    Column6 = reader.GetString(5),
                    Column7 = reader.GetString(6),
                    Column8 = reader.GetString(7),
                    Column9 = reader.GetString(8),
                    Interactionid = reader.GetInt32(9)
                });
            }

            _logger.LogInformation("Retrieved {Count} pending leads from database", leads.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending leads from database");
            throw;
        }

        return leads;
    }

    public async Task UpdateLeadStatusAsync(int interactionId, string message, string status)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_updatependingsibiellead_backup", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@interactionid", interactionId);
            command.Parameters.AddWithValue("@meassage", message);
            command.Parameters.AddWithValue("@status", status);

            await command.ExecuteNonQueryAsync();

            _logger.LogInformation("Updated lead status for InteractionId: {InteractionId}", interactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lead status for InteractionId: {InteractionId}", interactionId);
            throw;
        }
    }
}
