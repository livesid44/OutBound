# Database Migrations

This folder contains Entity Framework Core migrations for the Campaign Manager database.

## Overview

The application uses EF Core migrations to manage database schema changes. When you start the API, it automatically applies any pending migrations to the database.

## Migration Files

- **InitialCreate** - Creates the initial database schema with all tables and seed data

## How Migrations Work

When the API starts (in `Program.cs`), it:
1. Checks for pending migrations
2. Automatically applies them to the database
3. Logs the results

## Creating New Migrations

If you make changes to the models in `CampaignManager.Shared/Models/`, you need to create a new migration:

```bash
# Navigate to the API project
cd src/CampaignManager.Api

# Create a new migration (replace MigrationName with a descriptive name)
dotnet ef migrations add MigrationName --project ../CampaignManager.Data --context CampaignDbContext --output-dir Migrations
```

## Applying Migrations

Migrations are applied automatically when the API starts. Alternatively, you can apply them manually:

```bash
# Navigate to the API project
cd src/CampaignManager.Api

# Apply all pending migrations
dotnet ef database update --project ../CampaignManager.Data --context CampaignDbContext
```

## Removing the Last Migration

If you need to undo the last migration (before it's applied to production):

```bash
cd src/CampaignManager.Api
dotnet ef migrations remove --project ../CampaignManager.Data --context CampaignDbContext
```

## Viewing Migration SQL

To see what SQL will be executed for a migration:

```bash
cd src/CampaignManager.Api
dotnet ef migrations script --project ../CampaignManager.Data --context CampaignDbContext
```

## Important Notes

- **Never modify migration files after they've been applied to production databases**
- Always create a new migration for schema changes
- Test migrations on a development database before deploying to production
- The seed data is defined in `CampaignDbContext.OnModelCreating()` and is automatically included when creating migrations
- When you create a new database, the `InitialCreate` migration will automatically insert the seed data
