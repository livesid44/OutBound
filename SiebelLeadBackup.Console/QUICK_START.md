# Quick Start Guide

## First Time Setup

### 1. Deploy the Application

**Option A: Self-Contained (Recommended - No .NET Required)**
```cmd
cd SiebelLeadBackup.Console
publish.bat
# Select option 1
# Copy the 'publish' folder to your server
```

**Option B: Framework-Dependent (Requires .NET 8.0 Runtime)**
- Install .NET 8.0 Runtime: https://dotnet.microsoft.com/download/dotnet/8.0/runtime
```cmd
cd SiebelLeadBackup.Console
publish.bat
# Select option 2
# Copy the 'publish' folder to your server
```

### 2. Configure the Application

Copy and edit the configuration file:
```cmd
cd publish
copy appsettings.example.json appsettings.json
notepad appsettings.json
```

**Required Settings:**
```json
{
  "ConnectionString": "Server=YOUR_SQL_SERVER;Database=YOUR_DATABASE;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;",
  "SiebelApi": {
    "BaseUrl": "http://172.16.19.251:9001/siebel-rest/v1.0/service/CCCRMInbound/CCCRMInbound",
    "Authorization": "Basic Qk9CQ0NDUk1VU0VSOkJhbmtfMTIzNA==",
    "TimeoutSeconds": 30
  }
}
```

### 3. Test Database Connection

```sql
-- In SQL Server Management Studio, test:
EXEC usp_getpendingsibiellead_backup

-- Should return pending leads
```

### 4. Run the Application

**From Command Line (Recommended for first run):**
```cmd
cd D:\path\to\publish
SiebelLeadBackup.Console.exe
```

**Expected Output:**
```
=== Siebel Lead Backup Processing Application ===
Starting at: 12/16/2025 03:00:00 AM

Loading configuration from appsettings.json...
Configuration loaded successfully
...
Starting lead processing...
```

### 5. Check Logs

Logs are automatically created in:
```
publish\Logs\SiebelLeadBackup_2025-12-16.txt
```

## Common Issues & Quick Fixes

### Application Crashes Immediately

**Solution:** Run from Command Prompt to see error:
```cmd
cd D:\path\to\publish
SiebelLeadBackup.Console.exe
```

Look for error message, common issues:
- Missing `appsettings.json` → Copy from source
- Invalid `BaseUrl` → Check it's a valid URL
- Database connection error → Test SQL connection

### "Failed to load System.Private.CoreLib.dll"

**Solution:** Use self-contained deployment
```cmd
# In source directory
cd SiebelLeadBackup.Console
publish.bat
# Select option 1 (self-contained)
```

### No Logs Created

**Solution:** Check permissions
```cmd
# Run as Administrator or check folder permissions
icacls D:\path\to\publish /grant Users:(OI)(CI)F
```

### Processing Stops Early

**Solution:** Check MaxDegreeOfParallelism
```json
"Processing": {
  "MaxDegreeOfParallelism": 5  // Reduce from 20 if having issues
}
```

### Database Timeout

**Solution:** Application automatically uses 120 second timeout. 
If still timing out, check:
- Database server performance
- Network connectivity
- Large result sets (optimize stored procedure)

## Scheduling the Application

### Windows Task Scheduler

1. Open Task Scheduler
2. Create Basic Task
3. **Trigger:** Daily at desired time
4. **Action:** Start a program
5. **Program:** `D:\path\to\publish\SiebelLeadBackup.Console.exe`
6. **Start in:** `D:\path\to\publish`

### Windows Service (Using NSSM)

```cmd
# Download NSSM from https://nssm.cc/
nssm install SiebelLeadBackup "D:\path\to\publish\SiebelLeadBackup.Console.exe"
nssm set SiebelLeadBackup AppDirectory "D:\path\to\publish"
nssm start SiebelLeadBackup
```

## Configuration Reference

### Parallelism Settings

| Records | Recommended MaxDegreeOfParallelism |
|---------|-----------------------------------|
| < 500   | 5                                 |
| 500-3000| 10                                |
| 3000-9000| 20 (default)                     |
| > 9000  | 25-30                             |

### Logging Settings

```json
"Logging": {
  "LogFilePath": "Logs/SiebelLeadBackup_{Date}.txt",  // Daily rotation
  "EnableFileLogging": true,                          // Enable/disable
  "LogLevel": {
    "Default": "Information"  // Information, Debug, Warning, Error
  }
}
```

## Monitoring

### Check Application Status

**View Logs:**
```cmd
type D:\path\to\publish\Logs\SiebelLeadBackup_2025-12-16.txt
```

**Check Last Run:**
Look for these lines in the log:
```
=== Lead processing completed ===
Total leads processed: 9000
Successfully processed: 8950
Failed to process: 50
```

### Monitor Database

```sql
-- Check pending leads
SELECT COUNT(*) AS PendingCount
FROM [tbl_sibieldispositionapibackup]
WHERE flag=0 AND Freetext2 IS NOT NULL;

-- Check recently processed
SELECT TOP 100 *
FROM [tbl_sibieldispositionapibackup]
ORDER BY updated_date DESC;
```

## Performance Tips

1. **Optimize Parallelism:** Start with 5, increase gradually to find optimal value
2. **Database Indexes:** Ensure proper indexes on flag and Freetext2 columns
3. **Network:** Ensure stable connection to Siebel API
4. **Monitor Resources:** Watch CPU, memory, and network usage during processing
5. **Schedule Off-Peak:** Run during low-traffic hours for better performance

## Getting Help

**For detailed troubleshooting:** See `TROUBLESHOOTING.md`

**For setup instructions:** See `SETUP.md`

**For architecture details:** See `README.md`

**Critical Files:**
- `appsettings.json` - Configuration
- `Logs/*.txt` - Daily logs
- `TROUBLESHOOTING.md` - Diagnostic guide
- This file - Quick reference

## Success Checklist

- [ ] Application deployed to server
- [ ] appsettings.json configured with real values
- [ ] Database connection tested
- [ ] Application runs from command line without errors
- [ ] Logs directory created successfully
- [ ] Test run processes at least one lead
- [ ] Scheduled task/service configured (if needed)
- [ ] Monitoring set up (check logs daily)

## Version Information

- .NET Version: 8.0
- Default Parallelism: 20
- Default Timeout: 30 seconds (API), 120 seconds (Database)
- Log Rotation: Daily
