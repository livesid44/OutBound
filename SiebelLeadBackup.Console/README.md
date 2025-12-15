# Siebel Lead Backup Processing Console Application

## Overview

This is a standalone .NET console application designed to process pending Siebel leads from a backup table and submit them to the Siebel REST API. The application provides efficient, parallel processing with async/await patterns for optimal performance.

## Features

- **Async/Await Processing**: All database and API operations are asynchronous
- **Parallel Processing**: Configurable degree of parallelism to handle multiple leads simultaneously
- **Error Handling**: Comprehensive error handling with detailed logging
- **Status Tracking**: Updates database with success/failure status for each lead
- **Configurable**: All settings managed through `appsettings.json`

## Architecture

### Components

1. **Models**
   - `LeadData`: Represents a lead record from the database
   - `SiebelRequestBody`: Defines the structure for Siebel API requests
   - `AppSettings`: Configuration model

2. **Services**
   - `IDatabaseService` / `DatabaseService`: Handles database operations
   - `ISiebelApiService` / `SiebelApiService`: Manages HTTP communication with Siebel API
   - `ILeadProcessingService` / `LeadProcessingService`: Orchestrates the lead processing workflow

### Workflow

1. Retrieve pending leads using stored procedure `usp_getpendingsibiellead_backup`
2. Process leads in parallel (configurable parallelism)
3. For each lead:
   - Send HTTP POST request to Siebel API
   - Update database with result using stored procedure `usp_updatependingsibiellead_backup`

## Configuration

### appsettings.json

```json
{
  "ConnectionString": "Server=YOUR_SERVER;Database=YOUR_DATABASE;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;",
  "SiebelApi": {
    "BaseUrl": "http://172.16.19.251:9001/siebel-rest/v1.0/service/CCCRMInbound/CCCRMInbound",
    "Authorization": "Basic Qk9CQ0NDUk1VU0VSOkJhbmtfMTIzNA==",
    "TimeoutSeconds": 30
  },
  "Processing": {
    "MaxDegreeOfParallelism": 5,
    "BatchSize": 100
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning"
    }
  }
}
```

### Configuration Parameters

#### ConnectionString
- **Description**: SQL Server connection string
- **Format**: Standard SQL Server connection string
- **Required**: Yes

#### SiebelApi
- **BaseUrl**: The Siebel REST API endpoint
- **Authorization**: Basic authentication header value (Base64 encoded)
- **TimeoutSeconds**: HTTP request timeout in seconds (default: 30)

#### Processing
- **MaxDegreeOfParallelism**: Maximum number of concurrent lead processing operations (default: 5)
- **BatchSize**: Reserved for future batch processing enhancements (default: 100)

## Database Requirements

### Required Stored Procedures

#### 1. usp_getpendingsibiellead_backup

Retrieves pending leads from the backup table.

**Expected Output Columns:**
1. Column1 (Freetext1) - string
2. Column2 (Freetext2) - string
3. Column3 (Freetext3) - string
4. Column4 (Freetext4) - string
5. Column5 (Freetext5) - string (defaults to "-")
6. Column6 (Freetext6) - string
7. Column7 (Freetext7) - string
8. Column8 (Freetext8) - string
9. Column9 (Freetext9) - string
10. Interactionid - integer

**Expected Query:**
```sql
SELECT  ISNULL(Freetext1,'') AS Column1, 
        ISNULL(Freetext2,'') AS Column2,
        ISNULL(Freetext3,'') AS Column3,                            
        ISNULL(Freetext4,'') AS Column4,
        ISNULL('-', '') AS Column5,
        ISNULL(Freetext6,'') AS Column6,
        ISNULL(Freetext7,'') AS Column7,                      
        ISNULL(Freetext8,'') AS Column8,
        ISNULL(Freetext9,'') AS Column9, 
        Interactionid 
FROM   [tbl_sibieldispositionapibackup]
WHERE  flag=0 AND Freetext2 IS NOT NULL;
```

#### 2. usp_updatependingsibiellead_backup

Updates the lead status after processing.

**Parameters:**
- `@interactionid` (int): The interaction ID of the lead
- `@meassage` (string): Response message from the API or error message
- `@status` (string): "Success" or "Failed"

## API Integration

### Siebel REST API

**Endpoint:**
```
http://172.16.19.251:9001/siebel-rest/v1.0/service/CCCRMInbound/CCCRMInbound
```

**Method:** POST

**Headers:**
- `Authorization`: Basic Qk9CQ0NDUk1VU0VSOkJhbmtfMTIzNA==
- `Content-Type`: application/json

**Request Body Example:**
```json
{
   "Body": {
       "Freetext1": "Retail Liabilites",
       "Freetext2": "RLSA87431262502",
       "Freetext3": "Unable to connect",
       "Freetext4": "Call disconnected from customer side",
       "Freetext5": "-",
       "Freetext6": "12/04/2025 12:09:52",
       "Freetext7": "",
       "Freetext8": "0",
       "Freetext9": "Closure Remarks"
   }
}
```

## Building the Application

### Prerequisites
- .NET 10.0 SDK or later
- SQL Server access with required stored procedures

### Build Commands

```bash
# Restore dependencies
dotnet restore SiebelLeadBackup.sln

# Build the solution
dotnet build SiebelLeadBackup.sln

# Build in Release mode
dotnet build SiebelLeadBackup.sln --configuration Release
```

## Running the Application

### Development Run

```bash
cd SiebelLeadBackup.Console
dotnet run
```

### Production Run

```bash
cd SiebelLeadBackup.Console/bin/Release/net10.0
./SiebelLeadBackup.Console
```

### As a Windows Service or Scheduled Task

You can configure this console application to run as:
1. **Windows Scheduled Task**: Schedule periodic execution
2. **Windows Service**: Use a service wrapper like NSSM or create a Windows Service project
3. **Linux Cron Job**: Schedule execution on Linux systems

## Logging

The application uses Microsoft.Extensions.Logging with console output.

**Log Levels:**
- **Information**: General flow of the application
- **Warning**: Abnormal or unexpected events (e.g., API failures)
- **Error**: Errors and exceptions
- **Debug**: Detailed diagnostic information (HTTP requests/responses)

**Sample Output:**
```
=== Siebel Lead Backup Processing Application ===
Starting at: 12/15/2025 10:30:00 AM
Max Parallelism: 5

info: SiebelLeadBackup.Console.Services.DatabaseService[0]
      Retrieved 50 pending leads from database
info: SiebelLeadBackup.Console.Services.LeadProcessingService[0]
      Processing 50 leads with max parallelism of 5
info: SiebelLeadBackup.Console.Services.SiebelApiService[0]
      Successfully sent lead data for InteractionId: 12345
info: SiebelLeadBackup.Console.Services.DatabaseService[0]
      Updated lead status for InteractionId: 12345
info: SiebelLeadBackup.Console.Services.LeadProcessingService[0]
      Lead processing completed. Success: 48, Failed: 2

Completed at: 12/15/2025 10:31:15 AM
Press any key to exit...
```

## Performance Considerations

### Parallelism

The `MaxDegreeOfParallelism` setting controls how many leads are processed simultaneously:
- **Lower values (1-3)**: More conservative, suitable for limited resources or API rate limits
- **Medium values (4-8)**: Balanced performance and resource usage
- **Higher values (9+)**: Maximum throughput, requires adequate resources

**Recommendations:**
- Start with 5 and adjust based on:
  - Database server capacity
  - API server capacity and rate limits
  - Network bandwidth
  - Available CPU cores

### Error Handling

- Individual lead failures do not stop the entire batch
- Failed leads are logged and their status is updated in the database
- The application continues processing remaining leads even if some fail

## Troubleshooting

### Common Issues

1. **Connection String Error**
   - Verify SQL Server connection string in `appsettings.json`
   - Ensure SQL Server allows remote connections
   - Check firewall rules

2. **Stored Procedure Not Found**
   - Verify stored procedures exist in the database
   - Check stored procedure names match exactly
   - Ensure user has EXECUTE permission

3. **API Connection Timeout**
   - Increase `TimeoutSeconds` in configuration
   - Check network connectivity to API endpoint
   - Verify API endpoint is accessible

4. **Authorization Failure**
   - Verify the Authorization header value is correct
   - Ensure the Base64 encoded credentials are valid

## Security Considerations

1. **Connection String**: Store securely, consider using:
   - Environment variables
   - Azure Key Vault
   - Windows Credential Manager
   - Encrypted configuration files

2. **API Credentials**: The Authorization header contains Base64 encoded credentials
   - Current value: `BOBCCCRMUSER:Bank_1234`
   - Change default credentials in production
   - Consider using more secure authentication methods

3. **TrustServerCertificate**: Currently set to `True` for development
   - In production, use valid SSL certificates
   - Set to `False` and configure proper certificate validation

## License

This application is part of the OutBound repository by livesid44.

## Support

For issues or questions, please create an issue in the GitHub repository.
