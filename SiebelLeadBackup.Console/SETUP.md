# Setup Guide - Siebel Lead Backup Console Application

## Quick Start

Follow these steps to get the application up and running:

### 1. Prerequisites

Before you begin, ensure you have:

- **.NET 8.0 Runtime or SDK** installed ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- **SQL Server** access with the required stored procedures
- **Network access** to the Siebel API endpoint (http://172.16.19.251:9001)
- **Database permissions** to execute stored procedures

### 2. Configuration

1. **Copy the example configuration:**
   ```bash
   # On Windows
   copy appsettings.example.json appsettings.json
   
   # On Linux/Mac
   cp appsettings.example.json appsettings.json
   ```

2. **Edit `appsettings.json`** with your environment-specific values:

   ```json
   {
     "ConnectionString": "Server=YOUR_SQL_SERVER;Database=YOUR_DATABASE;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;",
     "SiebelApi": {
       "BaseUrl": "http://172.16.19.251:9001/siebel-rest/v1.0/service/CCCRMInbound/CCCRMInbound",
       "Authorization": "Basic Qk9CQ0NDUk1VU0VSOkJhbmtfMTIzNA==",
       "TimeoutSeconds": 30
     },
     "Processing": {
       "MaxDegreeOfParallelism": 5,
       "BatchSize": 100
     }
   }
   ```

### 3. Database Setup

Ensure the following stored procedures exist in your database:

#### A. usp_getpendingsibiellead_backup

This stored procedure should return pending leads:

```sql
CREATE PROCEDURE usp_getpendingsibiellead_backup
AS
BEGIN
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
END
```

#### B. usp_updatependingsibiellead_backup

This stored procedure should update the lead status after processing:

```sql
CREATE PROCEDURE usp_updatependingsibiellead_backup
    @interactionid INT,
    @meassage NVARCHAR(MAX),
    @status NVARCHAR(50)
AS
BEGIN
    -- Update the lead record with the API response
    UPDATE [tbl_sibieldispositionapibackup]
    SET 
        flag = CASE WHEN @status = 'Success' THEN 1 ELSE 2 END,
        response_message = @meassage,
        updated_date = GETDATE()
    WHERE Interactionid = @interactionid;
END
```

### 4. Verify Database Connection

Test your connection string using SQL Server Management Studio or Azure Data Studio before running the application.

### 5. Running the Application

#### Option A: Run from Source (Development)

```bash
cd SiebelLeadBackup.Console
dotnet run
```

#### Option B: Run Compiled Version

```bash
cd SiebelLeadBackup.Console/bin/Release/net8.0
dotnet SiebelLeadBackup.Console.dll
```

#### Option C: Publish and Deploy

**Windows:**
```batch
publish.bat
cd publish
SiebelLeadBackup.Console.exe
```

**Linux:**
```bash
chmod +x publish.sh
./publish.sh
cd publish
./SiebelLeadBackup.Console
```

## Deployment Options

### Option 1: Windows Scheduled Task

1. Publish the application using `publish.bat`
2. Copy the `publish` folder to your deployment location (e.g., `C:\Apps\SiebelLeadBackup`)
3. Update `appsettings.json` in the deployment folder
4. Open **Task Scheduler** (taskschd.msc)
5. Create a new task:
   - **Trigger**: Daily, or at desired intervals
   - **Action**: Start a program
   - **Program**: `C:\Apps\SiebelLeadBackup\SiebelLeadBackup.Console.exe`
   - **Start in**: `C:\Apps\SiebelLeadBackup`

### Option 2: Windows Service

1. Install a service wrapper like [NSSM](https://nssm.cc/)
2. Run as administrator:
   ```batch
   nssm install SiebelLeadBackup "C:\Apps\SiebelLeadBackup\SiebelLeadBackup.Console.exe"
   nssm set SiebelLeadBackup AppDirectory "C:\Apps\SiebelLeadBackup"
   nssm start SiebelLeadBackup
   ```

### Option 3: Linux Cron Job

1. Publish the application using `./publish.sh`
2. Copy the `publish` folder to `/opt/SiebelLeadBackup`
3. Update `appsettings.json`
4. Make the executable runnable: `chmod +x /opt/SiebelLeadBackup/SiebelLeadBackup.Console`
5. Edit crontab: `crontab -e`
6. Add a schedule (e.g., every 30 minutes):
   ```
   */30 * * * * cd /opt/SiebelLeadBackup && ./SiebelLeadBackup.Console >> /var/log/siebel-backup.log 2>&1
   ```

### Option 4: Linux Systemd Service

1. Create a service file `/etc/systemd/system/siebel-lead-backup.service`:

```ini
[Unit]
Description=Siebel Lead Backup Processing Service
After=network.target

[Service]
Type=oneshot
WorkingDirectory=/opt/SiebelLeadBackup
ExecStart=/opt/SiebelLeadBackup/SiebelLeadBackup.Console
User=your-service-user
StandardOutput=journal
StandardError=journal
SyslogIdentifier=siebel-lead-backup

[Install]
WantedBy=multi-user.target
```

2. Enable and start:
```bash
sudo systemctl daemon-reload
sudo systemctl enable siebel-lead-backup.timer
sudo systemctl start siebel-lead-backup.timer
```

3. Create a timer `/etc/systemd/system/siebel-lead-backup.timer`:

```ini
[Unit]
Description=Siebel Lead Backup Timer
Requires=siebel-lead-backup.service

[Timer]
OnBootSec=5min
OnUnitActiveSec=30min

[Install]
WantedBy=timers.target
```

## Configuration Reference

### Connection String Parameters

| Parameter | Description | Example |
|-----------|-------------|---------|
| Server | SQL Server hostname or IP | `localhost` or `10.0.0.5` |
| Database | Database name | `CampaignDB` |
| User Id | SQL Server username | `sa` or `app_user` |
| Password | SQL Server password | `YourPassword123` |
| TrustServerCertificate | Skip SSL validation (dev only) | `True` or `False` |
| Encrypt | Enable encryption | `True` or `False` |
| Connection Timeout | Connection timeout in seconds | `30` |

**Windows Authentication Example:**
```
Server=localhost;Database=CampaignDB;Trusted_Connection=True;TrustServerCertificate=True;
```

**SQL Authentication Example:**
```
Server=localhost;Database=CampaignDB;User Id=app_user;Password=SecurePass123;TrustServerCertificate=True;
```

### Processing Settings

| Setting | Description | Recommended Value |
|---------|-------------|-------------------|
| MaxDegreeOfParallelism | Concurrent operations | 5 (adjust based on load) |
| BatchSize | Reserved for future use | 100 |

**Guidelines for MaxDegreeOfParallelism:**
- **Low volume (<100 leads)**: 3-5
- **Medium volume (100-500 leads)**: 5-10
- **High volume (>500 leads)**: 10-20

### Logging Configuration

Available log levels: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`, `None`

```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "SiebelLeadBackup.Console.Services": "Debug",
    "Microsoft": "Warning",
    "System": "Warning"
  }
}
```

## Testing

### Test Database Connection

Before running the full application, test your database connection:

```bash
# Using sqlcmd (Windows)
sqlcmd -S YOUR_SERVER -U YOUR_USER -P YOUR_PASSWORD -d YOUR_DATABASE -Q "SELECT TOP 1 * FROM tbl_sibieldispositionapibackup"

# Using sqlcmd (Linux)
/opt/mssql-tools/bin/sqlcmd -S YOUR_SERVER -U YOUR_USER -P YOUR_PASSWORD -d YOUR_DATABASE -Q "SELECT TOP 1 * FROM tbl_sibieldispositionapibackup"
```

### Test API Connectivity

Test the Siebel API endpoint:

```bash
curl -X POST http://172.16.19.251:9001/siebel-rest/v1.0/service/CCCRMInbound/CCCRMInbound \
  -H "Authorization: Basic Qk9CQ0NDUk1VU0VSOkJhbmtfMTIzNA==" \
  -H "Content-Type: application/json" \
  -d '{
    "Body": {
      "Freetext1": "Test",
      "Freetext2": "TEST123",
      "Freetext3": "Test",
      "Freetext4": "Test",
      "Freetext5": "-",
      "Freetext6": "12/15/2025 10:00:00",
      "Freetext7": "",
      "Freetext8": "0",
      "Freetext9": "Test"
    }
  }'
```

### Run in Test Mode

For initial testing, set MaxDegreeOfParallelism to 1 to process leads sequentially:

```json
"Processing": {
  "MaxDegreeOfParallelism": 1,
  "BatchSize": 100
}
```

And enable debug logging:

```json
"Logging": {
  "LogLevel": {
    "Default": "Debug"
  }
}
```

## Monitoring

### View Logs

The application outputs logs to the console. To capture logs:

**Windows (PowerShell):**
```powershell
SiebelLeadBackup.Console.exe | Tee-Object -FilePath "logs\$(Get-Date -Format 'yyyy-MM-dd').log"
```

**Linux:**
```bash
./SiebelLeadBackup.Console >> logs/$(date +%Y-%m-%d).log 2>&1
```

### Database Monitoring

Monitor processing status:

```sql
-- Check pending leads
SELECT COUNT(*) AS PendingCount
FROM [tbl_sibieldispositionapibackup]
WHERE flag = 0 AND Freetext2 IS NOT NULL;

-- Check processed leads (last 24 hours)
SELECT 
    COUNT(*) AS ProcessedCount,
    SUM(CASE WHEN flag = 1 THEN 1 ELSE 0 END) AS SuccessCount,
    SUM(CASE WHEN flag = 2 THEN 1 ELSE 0 END) AS FailedCount
FROM [tbl_sibieldispositionapibackup]
WHERE updated_date >= DATEADD(HOUR, -24, GETDATE());
```

## Troubleshooting

### Common Issues

1. **"Connection string not found"**
   - Ensure `appsettings.json` exists in the same directory as the executable
   - Verify the JSON syntax is valid

2. **"Login failed for user"**
   - Verify SQL Server credentials
   - Check user has appropriate permissions
   - Ensure SQL Server allows remote connections

3. **"Could not find stored procedure"**
   - Verify stored procedures exist in the correct database
   - Check spelling and capitalization
   - Ensure user has EXECUTE permission

4. **"Connection timeout"**
   - Check network connectivity to SQL Server
   - Verify firewall allows SQL Server port (default: 1433)
   - Increase Connection Timeout in connection string

5. **"API endpoint not reachable"**
   - Verify network connectivity: `ping 172.16.19.251`
   - Check firewall allows port 9001
   - Test with curl command above

### Enable Verbose Logging

For detailed debugging, set all log levels to `Debug`:

```json
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "SiebelLeadBackup.Console.Services.DatabaseService": "Debug",
    "SiebelLeadBackup.Console.Services.SiebelApiService": "Debug",
    "SiebelLeadBackup.Console.Services.LeadProcessingService": "Debug"
  }
}
```

## Security Best Practices

1. **Protect Configuration Files**
   - Never commit `appsettings.json` with real credentials to source control
   - Use file system permissions to restrict access
   - Consider Azure Key Vault or environment variables for production

2. **Use Strong Passwords**
   - Change default API credentials
   - Use complex SQL Server passwords
   - Rotate credentials regularly

3. **Network Security**
   - Use VPN or private networks for database/API access
   - Enable TLS/SSL for SQL Server connections
   - Use HTTPS for API endpoints when available

4. **Audit and Monitoring**
   - Enable SQL Server auditing
   - Monitor application logs regularly
   - Set up alerts for failures

## Support

For issues, questions, or contributions:
- Create an issue in the GitHub repository
- Refer to the main [README.md](README.md) for architecture details

## License

Part of the OutBound repository by livesid44.
