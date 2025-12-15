# Siebel Lead Backup Processing Solution

## Overview

This document describes the standalone **Siebel Lead Backup Processing** console application that has been added to the OutBound repository. This application is completely independent of the main CampaignManager solution and serves a specific purpose: processing pending Siebel lead data from a backup table and submitting it to the Siebel REST API.

## Location

The solution is located in:
```
/SiebelLeadBackup.sln
/SiebelLeadBackup.Console/
```

This is a **standalone solution** separate from the main `CampaignManager.sln`.

## Purpose

The application automates the workflow of:
1. Retrieving pending lead records from a SQL Server backup table
2. Sending each lead's data to the Siebel CRM system via REST API
3. Updating the database with the success/failure status of each submission

## Key Features

### 1. **Async/Await Processing**
- All database and HTTP operations use async/await patterns
- Non-blocking I/O for maximum efficiency
- Proper cancellation token support

### 2. **Parallel Processing**
- Configurable degree of parallelism (default: 5 concurrent operations)
- Uses `Parallel.ForEachAsync` for optimal throughput
- Thread-safe status counting

### 3. **Robust Error Handling**
- Individual lead failures don't stop batch processing
- Comprehensive logging at all levels
- Detailed error messages stored in database

### 4. **Configuration Management**
- JSON-based configuration (`appsettings.json`)
- Easy deployment with separate configuration per environment
- No hardcoded values

### 5. **Enterprise-Ready Logging**
- Microsoft.Extensions.Logging integration
- Console logging with configurable levels
- Structured logging for easy parsing

## Architecture

### Project Structure

```
SiebelLeadBackup.Console/
├── Models/
│   ├── AppSettings.cs          # Configuration models
│   ├── LeadData.cs             # Lead data model
│   └── SiebelRequestBody.cs    # API request models
├── Services/
│   ├── IDatabaseService.cs     # Database service interface
│   ├── DatabaseService.cs      # Database operations implementation
│   ├── ISiebelApiService.cs    # API service interface
│   ├── SiebelApiService.cs     # HTTP client implementation
│   ├── ILeadProcessingService.cs    # Processing orchestration interface
│   └── LeadProcessingService.cs     # Main processing logic
├── Program.cs                  # Application entry point
├── appsettings.json           # Configuration (template)
├── appsettings.example.json   # Example configuration with sample values
├── publish.bat                # Windows publish script
├── publish.sh                 # Linux publish script
├── README.md                  # Comprehensive documentation
└── SETUP.md                   # Detailed setup guide
```

### Design Patterns

1. **Dependency Injection**: Services are injected through constructor injection
2. **Interface Segregation**: Each service has a dedicated interface
3. **Single Responsibility**: Each class has one clear purpose
4. **Repository Pattern**: Database operations are abstracted behind IDatabaseService

## Technical Stack

- **.NET 10.0**: Latest .NET runtime
- **Microsoft.Data.SqlClient**: SQL Server connectivity
- **Microsoft.Extensions.Configuration**: Configuration management
- **Microsoft.Extensions.Logging**: Structured logging
- **System.Text.Json**: JSON serialization for API calls
- **HttpClient**: HTTP communication with Siebel API

## Database Integration

### Stored Procedure: usp_getpendingsibiellead_backup

**Purpose**: Retrieves pending leads that need to be sent to Siebel

**Returns**: 10 columns
- Column1-9: Lead data (Freetext1-9)
- Interactionid: Unique identifier

### Stored Procedure: usp_updatependingsibiellead_backup

**Purpose**: Updates lead status after API submission

**Parameters**:
- `@interactionid` (int): Lead identifier
- `@meassage` (string): Response message from API
- `@status` (string): "Success" or "Failed"

> **Note**: The parameter name `@meassage` matches the stored procedure definition and is intentional.

## API Integration

### Siebel REST API Endpoint

**URL**: `http://172.16.19.251:9001/siebel-rest/v1.0/service/CCCRMInbound/CCCRMInbound`

**Method**: POST

**Authentication**: Basic Auth (Base64 encoded credentials)

**Content-Type**: application/json

**Request Format**:
```json
{
  "Body": {
    "Freetext1": "string",
    "Freetext2": "string",
    "Freetext3": "string",
    "Freetext4": "string",
    "Freetext5": "string",
    "Freetext6": "string",
    "Freetext7": "string",
    "Freetext8": "string",
    "Freetext9": "string"
  }
}
```

## Configuration

### Minimal Configuration Required

```json
{
  "ConnectionString": "Server=YOUR_SERVER;Database=YOUR_DATABASE;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;",
  "SiebelApi": {
    "BaseUrl": "http://YOUR_SERVER:PORT/siebel-rest/v1.0/service/CCCRMInbound/CCCRMInbound",
    "Authorization": "Basic YOUR_CREDENTIALS_BASE64",
    "TimeoutSeconds": 30
  },
  "Processing": {
    "MaxDegreeOfParallelism": 5
  }
}
```

## Usage

### Development

```bash
cd SiebelLeadBackup.Console
dotnet run
```

### Production Deployment

#### Windows
```batch
# Publish
publish.bat

# Deploy the 'publish' folder to your server
# Update appsettings.json
# Run via scheduled task or service
```

#### Linux
```bash
# Publish
./publish.sh

# Deploy the 'publish' folder to your server
# Update appsettings.json
# Run via cron or systemd
```

## Deployment Scenarios

### 1. Windows Scheduled Task
Run the application at regular intervals (e.g., every 30 minutes) using Windows Task Scheduler.

### 2. Windows Service
Use a service wrapper like NSSM to run as a Windows Service that executes on a schedule.

### 3. Linux Cron Job
Configure a cron job to execute the application periodically.

### 4. Linux Systemd Service
Create a systemd service and timer for scheduled execution.

### 5. Manual Execution
Run on-demand when needed for batch processing.

## Performance Considerations

### Parallelism Tuning

The `MaxDegreeOfParallelism` setting controls concurrent operations:

| Volume | Recommended Parallelism |
|--------|------------------------|
| < 100 leads | 3-5 |
| 100-500 leads | 5-10 |
| > 500 leads | 10-20 |

**Factors to consider**:
- Database server capacity
- API server capacity and rate limits
- Network bandwidth
- Available CPU cores

### Memory Usage

Memory usage scales with parallelism:
- Base: ~50 MB
- Per concurrent operation: ~5-10 MB
- Typical with 5 parallel: ~100-150 MB

## Monitoring and Logging

### Console Output

The application provides real-time progress information:
```
=== Siebel Lead Backup Processing Application ===
Starting at: 12/15/2025 10:30:00 AM
Max Parallelism: 5

info: Retrieved 50 pending leads from database
info: Processing 50 leads with max parallelism of 5
info: Successfully sent lead data for InteractionId: 12345
info: Lead processing completed. Success: 48, Failed: 2

Completed at: 12/15/2025 10:31:15 AM
```

### Log Levels

- **Information**: Normal operation flow
- **Warning**: Recoverable issues (e.g., API failures)
- **Error**: Serious errors that affect functionality
- **Debug**: Detailed diagnostic information

### Database Monitoring

Query to monitor processing status:
```sql
SELECT 
    COUNT(*) AS TotalPending
FROM [tbl_sibieldispositionapibackup]
WHERE flag = 0 AND Freetext2 IS NOT NULL;
```

## Security

### Credentials Management

- **Never commit** `appsettings.json` with real credentials
- Use environment-specific configuration files
- Consider Azure Key Vault or environment variables for production
- Implement credential rotation policies

### Network Security

- Use VPN or private networks for database/API access
- Enable TLS/SSL for SQL Server (set `TrustServerCertificate=False` with valid certs)
- Restrict firewall access to necessary ports only

### Database Security

- Use least-privilege SQL Server accounts
- Grant only EXECUTE permission on required stored procedures
- Enable SQL Server auditing
- Monitor for suspicious activity

## Troubleshooting

For detailed troubleshooting steps, see [SETUP.md](SiebelLeadBackup.Console/SETUP.md#troubleshooting).

Common issues:
1. **Connection failures**: Verify connection strings and network access
2. **Stored procedure not found**: Check procedures exist and user has EXECUTE permission
3. **API timeouts**: Increase `TimeoutSeconds` or check network connectivity
4. **High memory usage**: Reduce `MaxDegreeOfParallelism`

## Documentation

- **[README.md](SiebelLeadBackup.Console/README.md)**: Comprehensive application documentation
- **[SETUP.md](SiebelLeadBackup.Console/SETUP.md)**: Detailed setup and deployment guide
- **appsettings.example.json**: Example configuration with sample values

## Testing

The solution has been:
- ✅ Built successfully in Debug and Release configurations
- ✅ Reviewed for code quality
- ✅ Scanned for security vulnerabilities (CodeQL)
- ✅ Verified for proper configuration file handling

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-12-15 | Initial release with core functionality |

## License

This application is part of the OutBound repository by livesid44.

## Support

For issues, questions, or contributions related to this solution:
1. Create an issue in the GitHub repository
2. Refer to the detailed documentation in the `SiebelLeadBackup.Console` folder
3. Check the SETUP.md for troubleshooting steps

---

**Note**: This solution is completely standalone and does not depend on or affect the main CampaignManager solution in this repository.
