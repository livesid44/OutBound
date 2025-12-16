# Troubleshooting Guide - Siebel Lead Backup Console Application

## Application Crash (APPCRASH Event)

If the application crashes immediately on startup with an APPCRASH event in Windows Event Viewer, follow these steps:

### Step 1: Check Configuration File

The most common cause of crashes is missing or invalid configuration in `appsettings.json`.

**Verify the file exists:**
- The file `appsettings.json` must be in the same directory as `SiebelLeadBackup.Console.exe`
- If missing, copy it from the source or use `appsettings.example.json` as a template

**Check required settings:**
```json
{
  "ConnectionString": "Server=YOUR_SERVER;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASS;TrustServerCertificate=True;",
  "SiebelApi": {
    "BaseUrl": "http://YOUR_SERVER:PORT/path",
    "Authorization": "Basic YOUR_BASE64_CREDENTIALS",
    "TimeoutSeconds": 30
  },
  "Processing": {
    "MaxDegreeOfParallelism": 20
  },
  "Logging": {
    "LogFilePath": "Logs/SiebelLeadBackup_{Date}.txt",
    "EnableFileLogging": true
  }
}
```

### Step 2: Validate Configuration Values

**ConnectionString:**
- Must not be empty
- Must be a valid SQL Server connection string
- Test connection using SQL Server Management Studio first

**SiebelApi.BaseUrl:**
- Must be a valid URL (starts with `http://` or `https://`)
- Must not contain placeholder text like `YOUR_SERVER`
- Example: `http://172.16.19.251:9001/siebel-rest/v1.0/service/CCCRMInbound/CCCRMInbound`

**SiebelApi.Authorization:**
- Must not be empty
- Should start with "Basic " or just contain the Base64 encoded credentials
- Example: `Basic Qk9CQ0NDUk1VU0VSOkJhbmtfMTIzNA==`

### Step 3: Check Permissions

**Log Directory:**
- The application needs write permissions to create the `Logs` directory
- Ensure the user running the application has write access to the application directory
- Try running as Administrator to test if it's a permissions issue

**Database Permissions:**
- Ensure the SQL user has EXECUTE permission on:
  - `usp_getpendingsibiellead_backup`
  - `usp_updatependingsibiellead_backup`

### Step 4: Run from Command Line

Instead of double-clicking the .exe, run from Command Prompt or PowerShell to see error messages:

```cmd
cd D:\publish\publish
SiebelLeadBackup.Console.exe
```

or

```powershell
cd D:\publish\publish
.\SiebelLeadBackup.Console.exe
```

The application now displays detailed startup messages:
- Configuration loading status
- Service initialization status
- Any error messages with stack traces

### Step 5: Check .NET Runtime

**For Self-Contained Deployment:**
- Should work without .NET installed
- If crashing, the .exe might be corrupted - republish

**For Framework-Dependent Deployment:**
- Requires .NET 8.0 Runtime
- Download from: https://dotnet.microsoft.com/download/dotnet/8.0/runtime
- Verify installation: `dotnet --version` (should show 8.0.x)

### Step 6: Review Enhanced Error Messages

The updated application now provides:
- Detailed startup logging to console
- Step-by-step initialization messages
- Clear error messages if configuration is invalid
- Stack traces for all exceptions

**What you should see on successful startup:**
```
=== Siebel Lead Backup Processing Application ===
Starting at: 12/16/2025 03:00:00 AM
Application Path: D:\publish\publish

Loading configuration from appsettings.json...
Configuration loaded successfully
Parsing application settings...
Application settings parsed successfully
Setting up file logging...
Created log directory: D:\publish\publish\Logs
Log file path: D:\publish\publish\Logs/SiebelLeadBackup_2025-12-16.txt
Initializing logging system...
Logging system initialized successfully
Creating service instances...
All services created successfully

Max Parallelism: 20
Database Connection: Configured
Siebel API: http://172.16.19.251:9001/siebel-rest/v1.0/service/CCCRMInbound/CCCRMInbound

Starting lead processing...
```

### Step 7: Check for Specific Error Messages

**"ConnectionString not found in configuration"**
- Add or fix the `ConnectionString` key in appsettings.json

**"Invalid Siebel API BaseUrl"**
- Check that `SiebelApi.BaseUrl` is a valid URL
- Remove placeholder text like `YOUR_SIEBEL_SERVER:PORT`

**"Failed to load System.Private.CoreLib.dll"**
- Wrong .NET version - see Step 5
- Republish using the correct publish script

**"Could not load file or assembly"**
- Missing dependencies - use self-contained deployment
- Run: `publish.bat` and select option 1 (self-contained)

### Step 8: Check Log Files

If the application starts but crashes during processing:
1. Check the log file in `Logs/SiebelLeadBackup_YYYY-MM-DD.txt`
2. Look for the last log entry before crash
3. Common issues:
   - Database connection timeout
   - SQL query errors (missing columns, NULL values)
   - API endpoint unreachable
   - Network timeout

### Step 9: Test Database Connection

Test the stored procedure manually:

```sql
-- Test retrieval
EXEC usp_getpendingsibiellead_backup

-- Check if it returns data
SELECT TOP 10 * FROM [tbl_sibieldispositionapibackup]
WHERE flag=0 AND Freetext2 IS NOT NULL
```

If the stored procedure fails or returns unexpected data types, the application will crash.

### Step 10: Reduce Parallelism

High parallelism (20 concurrent operations) might overwhelm resources:

1. Open `appsettings.json`
2. Change `MaxDegreeOfParallelism` to a lower value:
```json
"Processing": {
  "MaxDegreeOfParallelism": 5
}
```
3. Restart the application

### Step 11: Test API Connectivity

Test the Siebel API manually using curl:

```cmd
curl -X POST "http://172.16.19.251:9001/siebel-rest/v1.0/service/CCCRMInbound/CCCRMInbound" ^
  -H "Authorization: Basic Qk9CQ0NDUk1VU0VSOkJhbmtfMTIzNA==" ^
  -H "Content-Type: application/json" ^
  -d "{\"Body\":{\"Freetext1\":\"Test\",\"Freetext2\":\"TEST\",\"Freetext3\":\"\",\"Freetext4\":\"\",\"Freetext5\":\"-\",\"Freetext6\":\"\",\"Freetext7\":\"\",\"Freetext8\":\"0\",\"Freetext9\":\"\"}}"
```

If this fails, the API endpoint is not accessible.

## Enhanced Error Handling (Latest Update)

The application has been updated with comprehensive error handling:

### Database Service
- ✅ NULL value checks for all columns
- ✅ Row-level error handling (skips bad rows)
- ✅ SQL exception logging with details
- ✅ Increased command timeout (120 seconds)
- ✅ Graceful failure for update operations

### API Service
- ✅ Validation of BaseUrl, Authorization on startup
- ✅ Proper URL parsing with error messages
- ✅ Timeout configuration with defaults
- ✅ Exception handling for all API calls

### Program Startup
- ✅ Step-by-step initialization logging
- ✅ Configuration validation
- ✅ URL validation before service creation
- ✅ Detailed error messages with stack traces
- ✅ Safe file logging initialization

### Processing Service
- ✅ Individual lead error handling
- ✅ Verification of all records processed
- ✅ Performance metrics and duration tracking
- ✅ Thread-safe counters for parallel processing

## Getting Help

If you're still experiencing crashes after following this guide:

1. **Collect Information:**
   - Console output (run from command line)
   - Log file contents from `Logs/` directory
   - appsettings.json (remove passwords)
   - Windows Event Viewer error details

2. **Check Requirements:**
   - Windows Server 2016 or later
   - SQL Server 2012 or later
   - Network access to Siebel API
   - Write permissions to application directory

3. **Try Self-Contained Deployment:**
   ```cmd
   # In source directory
   cd SiebelLeadBackup.Console
   publish.bat
   # Select option 1 (self-contained)
   ```

4. **Contact Support:**
   - Provide console output showing error
   - Include relevant log file entries
   - Describe when the crash occurs (startup vs. during processing)
