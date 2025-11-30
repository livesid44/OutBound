# Campaign Manager - Detailed Setup Guide

This guide provides step-by-step instructions for setting up the Campaign Manager application.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Database Setup](#database-setup)
3. [Application Configuration](#application-configuration)
4. [Running the Application](#running-the-application)
5. [Sample Users and Data](#sample-users-and-data)
6. [Troubleshooting](#troubleshooting)
7. [Production Deployment](#production-deployment)
8. [IIS Deployment](#iis-deployment)

---

## Prerequisites

### Required Software

| Software | Version | Download Link |
|----------|---------|---------------|
| .NET SDK | 8.0 or later | [Download](https://dotnet.microsoft.com/download/dotnet/8.0) |
| SQL Server | 2019+ (any edition) | See options below |

### SQL Server Options

Choose one of the following:

1. **SQL Server LocalDB** (Recommended for development)
   - Included with Visual Studio
   - Lightweight, no configuration needed
   - Connection string: `Server=(localdb)\mssqllocaldb;Database=CampaignManagerDb;Trusted_Connection=True;`

2. **SQL Server Express** (Free)
   - [Download SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)
   - Connection string: `Server=.\SQLEXPRESS;Database=CampaignManagerDb;Trusted_Connection=True;`

3. **SQL Server Developer Edition** (Free for development)
   - Full features, free for development
   - [Download Developer Edition](https://www.microsoft.com/sql-server/sql-server-downloads)

4. **Azure SQL Database** (Cloud)
   - Fully managed cloud database
   - Connection string: `Server=your-server.database.windows.net;Database=CampaignManagerDb;User Id=your-user;Password=your-password;`

---

## Database Setup

### Automatic Schema Creation

The application uses Entity Framework Core's `EnsureCreated()` method to **automatically create the database and all tables** when the API starts for the first time.

**No manual database setup is required!**

When you run the API:
1. It checks if the database exists
2. If not, it creates the database
3. It creates all required tables
4. It seeds sample data (users, project, campaign, etc.)

### Database Tables Created

| Table | Purpose |
|-------|---------|
| `Projects` | Multi-tenant organizations/projects |
| `Licenses` | License keys with usage limits |
| `Users` | User accounts (SuperAdmin, SubAdmin, Supervisor, Agent) |
| `Campaigns` | Campaign definitions |
| `CampaignFields` | Dynamic field schema per campaign |
| `CampaignApiConfigs` | SendGrid/WebEx API configurations |
| `CampaignStrategies` | Retry and scheduling rules |
| `CampaignLeads` | Lead data with status tracking |
| `LeadDispositions` | Call/contact outcomes |
| `Dispositions` | Disposition code master data |
| `ChatMessages` | Supervisor-agent chat messages |

### Manual Database Creation (Optional)

If you prefer to create the database manually:

```sql
CREATE DATABASE CampaignManagerDb;
GO
```

The tables will still be created automatically when the API starts.

---

## Application Configuration

### Step 1: API Configuration

Edit `src/CampaignManager.Api/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CampaignManagerDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "CampaignManager",
    "Audience": "CampaignManagerClients",
    "ExpiryInMinutes": 60
  }
}
```

**Important Settings:**

| Setting | Description |
|---------|-------------|
| `ConnectionStrings:DefaultConnection` | Your SQL Server connection string |
| `JwtSettings:SecretKey` | Secret key for JWT tokens (min 32 characters) |
| `JwtSettings:ExpiryInMinutes` | Token expiration time |

### Step 2: Blazor Web Configuration

Edit `src/CampaignManager.Web/wwwroot/appsettings.json`:

```json
{
  "ApiBaseUrl": "https://localhost:7001"
}
```

Update `ApiBaseUrl` to match your API's URL.

---

## Running the Application

### Option 1: Using Command Line

**Terminal 1 - Run the API:**
```bash
cd src/CampaignManager.Api
dotnet run
```

**Terminal 2 - Run the Blazor Web UI:**
```bash
cd src/CampaignManager.Web
dotnet run
```

### Option 2: Using Visual Studio

1. Open `CampaignManager.sln` in Visual Studio
2. Right-click on Solution → Set Startup Projects
3. Select "Multiple startup projects"
4. Set both `CampaignManager.Api` and `CampaignManager.Web` to "Start"
5. Press F5 to run

### Option 3: Using VS Code

1. Open the repository folder in VS Code
2. Install the C# extension
3. Press F5 and select the appropriate launch configuration

### Accessing the Application

| Component | URL |
|-----------|-----|
| API Swagger UI | https://localhost:7001/swagger |
| Blazor Web UI | https://localhost:5001 or http://localhost:5000 |

---

## Sample Users and Data

### Pre-configured Users

The application automatically seeds the following users:

| Role | Email | Password | Access Level |
|------|-------|----------|--------------|
| **SuperAdmin** | `admin@campaignmanager.local` | `Admin@123` | Full system access, create projects |
| **SubAdmin** | `subadmin@demo.local` | `SubAdmin@123` | Demo Project admin |
| **Supervisor** | `supervisor@demo.local` | `Supervisor@123` | Manage agents, view reports |
| **Agent** | `agent@demo.local` | `Agent@123` | Handle leads, make dispositions |

### Sample Project

A demo project is automatically created:

- **Name**: Demo Project
- **License Key**: `DEMO-1234-5678-ABCD`
- **Max Users**: 50
- **Max Campaigns**: 10
- **Expiry**: December 31, 2025

### Sample Campaign

A demo email campaign is created with:

- **Name**: Demo Email Campaign
- **Channel**: Email
- **Fields**: First Name, Last Name, Email, Phone
- **Strategy**: 3 email attempts, 5 call attempts

### Sample Dispositions

- Qualified Lead
- Not Interested
- Callback Required
- No Answer

---

## Troubleshooting

### Common Issues

#### 1. Database Connection Failed

**Error**: `A connection was successfully established with the server, but then an error occurred during the login handshake`

**Solution**: 
- Verify SQL Server is running
- Check connection string format
- Ensure database permissions

#### 2. CORS Error in Browser

**Error**: `Access to fetch at 'https://localhost:7001/api/...' has been blocked by CORS policy`

**Solution**:
- Ensure the API is running
- Check that the Blazor app URL is in the CORS policy
- Edit `Program.cs` in the API project if needed

#### 3. JWT Token Invalid

**Error**: `401 Unauthorized`

**Solution**:
- Ensure the JWT secret key is at least 32 characters
- Check token expiration
- Clear browser local storage and re-login

#### 4. Database Tables Not Created

**Solution**:
- Delete the database and restart the API
- Check for migration errors in the console
- Verify connection string is correct

### Getting Help

If you encounter issues:
1. Check the API console output for errors
2. Review the browser developer console (F12)
3. Verify all configuration settings
4. Create an issue on GitHub with error details

---

## Production Deployment

For production deployment:

1. **Security**:
   - Change all default passwords
   - Use strong JWT secret keys
   - Store secrets in Azure Key Vault or environment variables
   - Enable HTTPS

2. **Database**:
   - Use a production SQL Server instance
   - Configure appropriate connection pooling
   - Set up database backups

3. **CORS**:
   - Update CORS policy to only allow your production domain

4. **Logging**:
   - Configure appropriate log levels
   - Set up centralized logging (e.g., Application Insights)

---

## IIS Deployment

### Prerequisites for IIS

1. **Install the ASP.NET Core Hosting Bundle**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Select "Hosting Bundle" under "Run apps - Runtime"
   - This installs the .NET Runtime, .NET Core Runtime, and the ASP.NET Core Module

2. **Enable IIS Features**
   - Windows Features → Internet Information Services
   - Enable: Web Management Tools, World Wide Web Services

3. **Install URL Rewrite Module** (for Blazor WebAssembly)
   - Download from: https://www.iis.net/downloads/microsoft/url-rewrite

### Deploy the API (CampaignManager.Api)

1. **Publish the API:**
   ```bash
   cd src/CampaignManager.Api
   dotnet publish -c Release -o ./publish
   ```

2. **Create IIS Site:**
   - Open IIS Manager
   - Right-click on Sites → Add Website
   - Site name: `CampaignManagerApi`
   - Physical path: Point to the `publish` folder
   - Port: 7001 (or your preferred port)
   - Application Pool: Create new pool with "No Managed Code"

3. **Configure Application Pool:**
   - Select the application pool
   - Advanced Settings → Identity → Set to a user with database access

### Deploy the Blazor Web UI (CampaignManager.Web)

1. **Publish the Web UI:**
   ```bash
   cd src/CampaignManager.Web
   dotnet publish -c Release -o ./publish
   ```

2. **Create IIS Site:**
   - Open IIS Manager
   - Right-click on Sites → Add Website
   - Site name: `CampaignManagerWeb`
   - Physical path: Point to the `publish/wwwroot` folder
   - Port: 5001 (or your preferred port)

3. **Important**: The `web.config` in `wwwroot` handles:
   - MIME types for .wasm, .dll, and other Blazor files
   - SPA fallback routing for client-side navigation
   - Compression settings

### Troubleshooting IIS Deployment

#### HTTP 500.19 - Invalid Configuration

**Common Causes:**
1. Missing URL Rewrite Module
2. Invalid web.config syntax
3. Missing ASP.NET Core Hosting Bundle

**Solutions:**
1. Install URL Rewrite Module: https://www.iis.net/downloads/microsoft/url-rewrite
2. Install ASP.NET Core Hosting Bundle
3. Verify web.config is valid XML
4. Check IIS Manager → Error Pages for detailed errors

#### HTTP 502.5 - Process Failure

**Solutions:**
1. Verify .NET 8 Runtime is installed
2. Check the `stdout` log in the `logs` folder
3. Run the application from command line to see errors:
   ```bash
   cd publish
   dotnet CampaignManager.Api.dll
   ```

#### Blazor WASM Files Not Loading

**Solutions:**
1. Verify MIME types are configured in web.config
2. Check that URL Rewrite Module is installed
3. Ensure `web.config` is in the `wwwroot` folder

### IIS Configuration Files

The repository includes pre-configured `web.config` files:

- `src/CampaignManager.Api/web.config` - For ASP.NET Core API hosting
- `src/CampaignManager.Web/wwwroot/web.config` - For Blazor WebAssembly hosting

These files are automatically copied during publish.
