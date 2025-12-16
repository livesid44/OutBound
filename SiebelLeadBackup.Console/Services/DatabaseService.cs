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
            _logger.LogInformation("Attempting to connect to database...");
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            _logger.LogInformation("Database connection established successfully");

            using var command = new SqlCommand("usp_getpendingsibiellead_backup", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure,
                CommandTimeout = 120 // 2 minutes timeout
            };

            _logger.LogInformation("Executing stored procedure: usp_getpendingsibiellead_backup");
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                try
                {
                    leads.Add(new LeadData
                    {
                        Column1 = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                        Column2 = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        Column3 = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        Column4 = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        Column5 = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                        Column6 = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                        Column7 = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                        Column8 = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                        Column9 = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                        Interactionid = reader.GetInt32(9)
                    });
                }
                catch (Exception rowEx)
                {
                    _logger.LogError(rowEx, "Error reading row data, skipping this row");
                    continue;
                }
            }

            _logger.LogInformation("Retrieved {Count} pending leads from database", leads.Count);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL error retrieving pending leads. Error: {Error}", sqlEx.Message);
            throw;
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
                CommandType = System.Data.CommandType.StoredProcedure,
                CommandTimeout = 60
            };

            // Truncate message if too long to prevent database errors
            var truncatedMessage = message?.Length > 4000 ? message.Substring(0, 4000) : message ?? string.Empty;

            command.Parameters.AddWithValue("@interactionid", interactionId);
            command.Parameters.AddWithValue("@meassage", truncatedMessage);
            command.Parameters.AddWithValue("@status", status ?? "Unknown");

            await command.ExecuteNonQueryAsync();

            _logger.LogDebug("Updated lead status for InteractionId: {InteractionId} to {Status}", interactionId, status);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL error updating lead status for InteractionId: {InteractionId}. Error: {Error}", interactionId, sqlEx.Message);
            // Don't throw - allow processing to continue for other leads
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lead status for InteractionId: {InteractionId}", interactionId);
            // Don't throw - allow processing to continue for other leads
        }
    }
}
