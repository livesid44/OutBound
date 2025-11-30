# Campaign Manager

A comprehensive multi-tenant campaign management system built with modern Microsoft stack (Blazor WebAssembly + ASP.NET Core API + Entity Framework Core + SQL Server).

## Features

### 1. Multi-Tenant System
- SuperAdmin can create projects/tenants with licensing
- License key activation with user and campaign limits
- Pre-login registration with license key

### 2. User Management
- Individual and bulk user creation
- Role-based access: SuperAdmin, SubAdmin, Supervisor, Agent
- Supervisor-agent mapping

### 3. Campaign Designer
- Basic details (name, description, channel selection)
- Dynamic field definition for campaign data
- API configuration for:
  - **Email**: SendGrid integration
  - **Voice/Call**: WebEx CURL with JSON parameter mapping
- Strategy definition (retry attempts, intervals, working hours)

### 4. Scheduler & Monitor
- Toggle scheduler on/off per campaign
- Real-time queue monitoring:
  - Email queue vs Voice queue
  - Lead status: Queued → Dialed → Connected → RPC → Disposed

### 5. Reports & Dashboard
- Campaign performance overview
- Custom report designer
- Supervisor dashboard with team performance
- Agent online status tracking

### 6. Agent UI
- WebEx CTI integration
- Customer information display
- Disposition form with callback scheduling
- Supervisor chat

### 7. Data Upload
- Campaign-specific field templates
- Excel/CSV file upload
- API endpoint for bulk data push

## Project Structure

```
CampaignManager/
├── src/
│   ├── CampaignManager.Shared/       # Shared models and DTOs
│   ├── CampaignManager.Data/         # Entity Framework DbContext and repositories
│   ├── CampaignManager.Api/          # ASP.NET Core Web API
│   └── CampaignManager.Web/          # Blazor WebAssembly UI
└── CampaignManager.sln
```

## Technology Stack

- **Frontend**: Blazor WebAssembly (.NET 8)
- **Backend**: ASP.NET Core 8 Web API
- **Database**: Microsoft SQL Server with Entity Framework Core
- **Authentication**: JWT Bearer tokens
- **Email**: SendGrid integration
- **Voice**: WebEx CTI integration

---

## 🚀 Quick Start Setup Guide

### Prerequisites

1. **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **SQL Server** - One of the following:
   - SQL Server LocalDB (included with Visual Studio)
   - SQL Server Express (free)
   - SQL Server Developer Edition (free)
   - Azure SQL Database

### Step 1: Clone the Repository

```bash
git clone https://github.com/livesid44/mail-graph-sqlite.git
cd mail-graph-sqlite
```

### Step 2: Configure the Database Connection

Update the connection string in `src/CampaignManager.Api/appsettings.json`:

```json
{
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

**Connection String Examples:**

| Database Type | Connection String |
|--------------|-------------------|
| LocalDB | `Server=(localdb)\\mssqllocaldb;Database=CampaignManagerDb;Trusted_Connection=True;` |
| SQL Server Express | `Server=.\\SQLEXPRESS;Database=CampaignManagerDb;Trusted_Connection=True;` |
| SQL Server (Windows Auth) | `Server=YOUR_SERVER;Database=CampaignManagerDb;Trusted_Connection=True;` |
| SQL Server (SQL Auth) | `Server=YOUR_SERVER;Database=CampaignManagerDb;User Id=sa;Password=YourPassword;` |

### Step 3: Configure the Blazor Frontend

Update API URL in `src/CampaignManager.Web/wwwroot/appsettings.json`:

```json
{
  "ApiBaseUrl": "https://localhost:7001"
}
```

### Step 4: Build the Solution

```bash
dotnet build CampaignManager.sln
```

### Step 5: Run the API (First Terminal)

```bash
cd src/CampaignManager.Api
dotnet run
```

The API will:
1. **Automatically create the database** if it doesn't exist
2. **Create all required tables** (Projects, Users, Campaigns, Leads, etc.)
3. **Seed sample data** including users, a demo project, and sample campaign

### Step 6: Run the Blazor Web UI (Second Terminal)

```bash
cd src/CampaignManager.Web
dotnet run
```

### Step 7: Access the Application

- **API Swagger UI**: https://localhost:7001/swagger
- **Blazor Web UI**: https://localhost:5001 (or http://localhost:5000)

---

## 👤 Pre-configured Sample Users

The system automatically creates the following sample users for testing:

| Role | Email | Password | Description |
|------|-------|----------|-------------|
| **SuperAdmin** | `admin@campaignmanager.local` | `Admin@123` | Full system access, can create projects |
| **SubAdmin** | `subadmin@demo.local` | `SubAdmin@123` | Project admin for Demo Project |
| **Supervisor** | `supervisor@demo.local` | `Supervisor@123` | Manages agents in Demo Project |
| **Agent** | `agent@demo.local` | `Agent@123` | Call center agent in Demo Project |

### Sample Data Included

When the API starts, it automatically creates:

1. **Demo Project** - A sample tenant with:
   - License Key: `DEMO-1234-5678-ABCD`
   - Max Users: 50
   - Max Campaigns: 10
   - Expiry: December 31, 2025

2. **Demo Email Campaign** - Pre-configured with:
   - Fields: First Name, Last Name, Email, Phone
   - Strategy: 3 email attempts, 5 call attempts
   - Working days: Monday-Friday

3. **Sample Dispositions**:
   - Qualified Lead
   - Not Interested
   - Callback Required
   - No Answer

---

## 🔧 Database Schema

The application automatically creates the following tables:

| Table | Description |
|-------|-------------|
| `Projects` | Multi-tenant projects/organizations |
| `Licenses` | License keys with user/campaign limits |
| `Users` | User accounts with roles |
| `Campaigns` | Campaign definitions |
| `CampaignFields` | Dynamic field schema per campaign |
| `CampaignApiConfigs` | SendGrid/WebEx configurations |
| `CampaignStrategies` | Retry and scheduling rules |
| `CampaignLeads` | Lead data with status tracking |
| `LeadDispositions` | Call/contact outcomes |
| `Dispositions` | Disposition code master data |
| `ChatMessages` | Supervisor-agent chat |

---

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/activate-license` - Activate license key

### Projects (SuperAdmin only)
- `GET /api/projects` - List projects
- `POST /api/projects` - Create project with license
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Delete project

### Users
- `GET /api/users` - List users
- `POST /api/users` - Create user
- `POST /api/users/bulk` - Bulk create users
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

### Campaigns
- `GET /api/campaigns` - List campaigns
- `POST /api/campaigns` - Create campaign
- `GET /api/campaigns/{id}` - Get campaign details
- `PUT /api/campaigns/{id}` - Update campaign
- `DELETE /api/campaigns/{id}` - Delete campaign
- `GET /api/campaigns/{id}/fields` - Get campaign fields
- `POST /api/campaigns/{id}/fields` - Add field
- `PUT /api/campaigns/{id}/api-config` - Update API configuration
- `PUT /api/campaigns/{id}/strategy` - Update strategy
- `POST /api/campaigns/{id}/scheduler/toggle` - Toggle scheduler

### Leads
- `GET /api/campaigns/{id}/leads` - List leads
- `POST /api/campaigns/{id}/leads` - Create lead
- `POST /api/campaigns/{id}/leads/bulk` - Bulk upload leads
- `POST /api/leads/{id}/dispose` - Dispose lead

### Dashboard
- `GET /api/dashboard/summary` - Dashboard summary
- `GET /api/dashboard/supervisor` - Supervisor dashboard
- `GET /api/dashboard/agent-performance` - Agent performance

---

## 🔒 Security Notes

- Never commit secrets to source control
- Use Azure Key Vault or environment variables for production secrets
- Change default passwords immediately for production use
- Configure CORS appropriately for production
- Use HTTPS in production

---

## Legacy Code

The original `mail-graph-sqlite` console application files remain in the repository root for reference:
- `Program.cs` - Original console app
- `Database.cs`, `EmailRecord.cs`, `GraphHelper.cs`, `GraphAuthProvider.cs` - Original Graph API integration