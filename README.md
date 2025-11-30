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

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or full instance)

### Configuration

1. Update connection string in `src/CampaignManager.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CampaignManagerDb;..."
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "CampaignManager",
    "Audience": "CampaignManagerClients"
  }
}
```

2. Update API URL in `src/CampaignManager.Web/wwwroot/appsettings.json`:
```json
{
  "ApiBaseUrl": "https://localhost:7001"
}
```

### Build & Run

```bash
# Build solution
dotnet build CampaignManager.sln

# Run API
cd src/CampaignManager.Api
dotnet run

# Run Blazor Web (in another terminal)
cd src/CampaignManager.Web
dotnet run
```

### Default Admin Account
- Email: `admin@campaignmanager.local`
- Password: `Admin@123`

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

## Legacy Code

The original `mail-graph-sqlite` console application files remain in the repository root for reference:
- `Program.cs` - Original console app
- `Database.cs`, `EmailRecord.cs`, `GraphHelper.cs`, `GraphAuthProvider.cs` - Original Graph API integration

## Security Notes

- Never commit secrets to source control
- Use Azure Key Vault or environment variables for production secrets
- Change default admin password immediately
- Configure CORS appropriately for production