# Siebel Lead Backup Processing Console Application

## Overview

This is a standalone .NET console application designed to process pending Siebel leads from a backup table and submit them to the Siebel REST API. The application provides efficient, parallel processing with async/await patterns for optimal performance.

## Features

- **Async/Await Processing**: All database and API operations are asynchronous
- **High-Volume Parallel Processing**: Configurable degree of parallelism (default: 20) to handle large volumes (9000+ records) efficiently
- **Complete Processing Guarantee**: Ensures all records are processed with detailed tracking and verification
- **File Logging**: Automatic logging to text files in the Logs directory with daily rotation
- **Comprehensive Tracking**: Progress tracking for each lead with counts (e.g., "Processing 1532/9000")
- **Error Handling**: Comprehensive error handling with detailed logging
- **Status Tracking**: Updates database with success/failure status for each lead
- **Performance Metrics**: Duration tracking and average time per lead calculation
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
    "MaxDegreeOfParallelism": 20,
    "BatchSize": 100
  },
  "Logging": {
    "LogFilePath": "Logs/SiebelLeadBackup_{Date}.txt",
    "EnableFileLogging": true,
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
- **MaxDegreeOfParallelism**: Maximum number of concurrent lead processing operations (default: 20)
  - Set higher (20-30) for processing large volumes (e.g., 9000 records)
  - Adjust based on server capacity and network bandwidth
- **BatchSize**: Reserved for future batch processing enhancements (default: 100)

#### Logging
- **LogFilePath**: Path to the log file with {Date} placeholder (default: "Logs/SiebelLeadBackup_{Date}.txt")
  - {Date} is automatically replaced with current date in yyyy-MM-dd format
  - Creates daily log files (e.g., SiebelLeadBackup_2025-12-15.txt)
- **EnableFileLogging**: Enable/disable file logging (default: true)
- **LogLevel**: Configure logging verbosity for different components

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
- .NET 8.0 SDK or later
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

## Publishing for Deployment

### Important: Deployment Options

To deploy the application to a server, you have two options:

#### Option 1: Self-Contained (Recommended for Production)
**Includes the .NET runtime with the application. No .NET installation required on the target machine.**

```bash
# Windows - Run publish.bat and select option 1
publish.bat

# Linux - Run publish.sh and select option 1
./publish.sh

# Or manually:
dotnet publish --configuration Release --output ./publish --self-contained true --runtime win-x64 -p:PublishSingleFile=true
```

**Pros:**
- No .NET runtime installation needed on target machine
- Guaranteed runtime version compatibility
- Single .exe file (with PublishSingleFile)

**Cons:**
- Larger file size (~70-80 MB)

#### Option 2: Framework-Dependent
**Requires .NET 8.0 Runtime to be installed on the target machine.**

```bash
# Windows - Run publish.bat and select option 2
publish.bat

# Linux - Run publish.sh and select option 2
./publish.sh

# Or manually:
dotnet publish --configuration Release --output ./publish --self-contained false --runtime win-x64
```

**Pros:**
- Smaller file size (~500 KB)

**Cons:**
- Requires .NET 8.0 Runtime installed on target machine
- Download from: https://dotnet.microsoft.com/download/dotnet/8.0

### Resolving "Failed to load System.Private.CoreLib.dll" Error

If you encounter the error:
```
Failed to load System.Private.CoreLib.dll (error code 0x800700C1)
Could not load file or assembly. is not a valid Win32 application.
```

**This means the .NET runtime is not installed or incompatible. Solutions:**

1. **Use Self-Contained Deployment (Easiest)**: Re-publish using the self-contained option above. This bundles the runtime with your application.

2. **Install .NET 8.0 Runtime**: Download and install the .NET 8.0 Runtime from:
   - Windows: https://dotnet.microsoft.com/download/dotnet/8.0/runtime
   - Ensure you install the correct architecture (x64 for 64-bit Windows)

## Running the Application

### Development Run

```bash
cd SiebelLeadBackup.Console
dotnet run
```

### Production Run (Framework-Dependent)

```bash
cd SiebelLeadBackup.Console/bin/Release/net8.0
dotnet SiebelLeadBackup.Console.dll
```

### Production Run (Self-Contained)

```bash
cd publish
./SiebelLeadBackup.Console.exe   # Windows
./SiebelLeadBackup.Console       # Linux
```

### As a Windows Service or Scheduled Task

You can configure this console application to run as:
1. **Windows Scheduled Task**: Schedule periodic execution
2. **Windows Service**: Use a service wrapper like NSSM or create a Windows Service project
3. **Linux Cron Job**: Schedule execution on Linux systems

## Logging

The application uses Microsoft.Extensions.Logging with dual output:
1. **Console Output**: Real-time progress display
2. **File Output**: Persistent logs saved to text files in the `Logs` directory

**Log File Location:**
- Default: `Logs/SiebelLeadBackup_YYYY-MM-DD.txt`
- Daily rotation: New file created each day
- Example: `Logs/SiebelLeadBackup_2025-12-15.txt`

**Log Levels:**
- **Information**: General flow of the application, progress tracking
- **Warning**: Abnormal or unexpected events (e.g., API failures)
- **Error**: Errors and exceptions
- **Debug**: Detailed diagnostic information (HTTP requests/responses)

**Enhanced Features:**
- **Progress Tracking**: Shows current/total for each lead (e.g., "Processing 1532/9000")
- **Completion Verification**: Warns if not all records were processed
- **Performance Metrics**: Duration and average time per lead
- **Detailed Status**: Success/failure for each individual lead

**Sample Output:**
```
=== Siebel Lead Backup Processing Application ===
Starting at: 12/15/2025 10:30:00 AM
Max Parallelism: 20

info: === Starting lead processing ===
info: Timestamp: 12/15/2025 10:30:01 AM
info: Retrieved 9000 pending leads from database
info: Processing with max parallelism of 20
info: Expected to process all 9000 records

info: Processing lead 1/9000 - InteractionId: 12345
info: Lead 12345 processed successfully (1/9000)
info: Processing lead 2/9000 - InteractionId: 12346
info: Lead 12346 processed successfully (2/9000)
...
info: Processing lead 9000/9000 - InteractionId: 21344
info: Lead 21344 processed successfully (9000/9000)

info: === Lead processing completed ===
info: Total leads retrieved: 9000
info: Total leads processed: 9000
info: Successfully processed: 8950
info: Failed to process: 50
info: Processing duration: 00:15:30
info: Average time per lead: 103.33 ms

Completed at: 12/15/2025 10:45:31 AM
Press any key to exit...
```

## Performance Considerations

### Parallelism

The `MaxDegreeOfParallelism` setting controls how many leads are processed simultaneously:
- **Low volume (<500 records)**: 5-10 concurrent operations
- **Medium volume (500-3000 records)**: 10-15 concurrent operations
- **High volume (3000-10000+ records)**: 20-30 concurrent operations (default: 20)

**Optimized for Large Volumes:**
- Default setting of 20 is designed to efficiently handle 9000+ records
- Ensures all records are processed completely
- Provides detailed progress tracking (e.g., "Processing 5432/9000")

**Recommendations:**
- Current default (20) is optimized for high-volume processing
- Increase to 25-30 for even faster processing if:
  - Database server has high capacity
  - API server can handle higher concurrent requests
  - Network bandwidth is sufficient
  - Server has 8+ CPU cores
- Decrease to 10-15 if experiencing:
  - API rate limiting
  - Database connection pool exhaustion
  - Memory pressure

### Processing Guarantee

- **Complete Loop Execution**: Application processes all retrieved records
- **Progress Verification**: Logs warn if processed count doesn't match total count
- **Individual Tracking**: Each lead is tracked with InteractionId and position (current/total)
- **Performance Metrics**: Reports total duration and average time per lead

### Error Handling

- Individual lead failures do not stop the entire batch
- Failed leads are logged with detailed error messages
- Database status is updated for both successful and failed leads
- The application continues processing remaining leads even if some fail
- Summary report shows exact success/failure counts

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
