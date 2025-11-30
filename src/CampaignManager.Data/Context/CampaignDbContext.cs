using CampaignManager.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CampaignManager.Data.Context;

public class CampaignDbContext : DbContext
{
    public CampaignDbContext(DbContextOptions<CampaignDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<License> Licenses => Set<License>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignField> CampaignFields => Set<CampaignField>();
    public DbSet<CampaignApiConfig> CampaignApiConfigs => Set<CampaignApiConfig>();
    public DbSet<CampaignStrategy> CampaignStrategies => Set<CampaignStrategy>();
    public DbSet<CampaignLead> CampaignLeads => Set<CampaignLead>();
    public DbSet<LeadDisposition> LeadDispositions => Set<LeadDisposition>();
    public DbSet<Disposition> Dispositions => Set<Disposition>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Project configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasIndex(e => e.Name);
            entity.HasOne(e => e.License)
                  .WithOne(l => l.Project)
                  .HasForeignKey<License>(l => l.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // License configuration
        modelBuilder.Entity<License>(entity =>
        {
            entity.HasIndex(e => e.LicenseKey).IsUnique();
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.Users)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Supervisor)
                  .WithMany(s => s.Subordinates)
                  .HasForeignKey(e => e.SupervisorId)
                  .OnDelete(DeleteBehavior.NoAction);  // Changed to NoAction to avoid cascade cycles
        });

        // Campaign configuration
        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.Status);
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.Campaigns)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ApiConfig)
                  .WithOne(a => a.Campaign)
                  .HasForeignKey<CampaignApiConfig>(a => a.CampaignId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Strategy)
                  .WithOne(s => s.Campaign)
                  .HasForeignKey<CampaignStrategy>(s => s.CampaignId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // CampaignField configuration
        modelBuilder.Entity<CampaignField>(entity =>
        {
            entity.HasIndex(e => new { e.CampaignId, e.FieldName }).IsUnique();
            entity.HasOne(e => e.Campaign)
                  .WithMany(c => c.Fields)
                  .HasForeignKey(e => e.CampaignId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // CampaignLead configuration
        modelBuilder.Entity<CampaignLead>(entity =>
        {
            entity.HasIndex(e => e.CampaignId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.NextScheduledAction);
            entity.HasOne(e => e.Campaign)
                  .WithMany(c => c.Leads)
                  .HasForeignKey(e => e.CampaignId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.AssignedAgent)
                  .WithMany()
                  .HasForeignKey(e => e.AssignedAgentId)
                  .OnDelete(DeleteBehavior.NoAction);  // Avoid cascade conflicts with User table
            entity.HasOne(e => e.Disposition)
                  .WithOne(d => d.Lead)
                  .HasForeignKey<LeadDisposition>(d => d.LeadId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // LeadDisposition configuration
        modelBuilder.Entity<LeadDisposition>(entity =>
        {
            entity.HasOne(e => e.DisposedBy)
                  .WithMany()
                  .HasForeignKey(e => e.DisposedById)
                  .OnDelete(DeleteBehavior.NoAction);  // Avoid cascade conflicts with User table
        });

        // Disposition configuration
        modelBuilder.Entity<Disposition>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.Code }).IsUnique();
            entity.HasOne(e => e.Parent)
                  .WithMany(p => p.SubDispositions)
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ChatMessage configuration
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasIndex(e => new { e.SenderId, e.ReceiverId });
            entity.HasIndex(e => e.SentAt);
            entity.HasOne(e => e.Sender)
                  .WithMany()
                  .HasForeignKey(e => e.SenderId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Receiver)
                  .WithMany()
                  .HasForeignKey(e => e.ReceiverId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Pre-computed BCrypt hashes for seed data (prevents hash regeneration on each startup)
        // These are static values - the original passwords are documented in README.md and SETUP.md
        const string adminPasswordHash = "$2a$11$WCDqnEksHWQsG7Za1KfMue9sOUHGRPWPV3GKL93AYQcJzIn8oU7ui"; // Admin@123
        const string subAdminPasswordHash = "$2a$11$wbeagR.onmQYakqSy5tEqO4z6U.E4JCFqndvYL.6OG5hNj6xsKcfi"; // SubAdmin@123
        const string supervisorPasswordHash = "$2a$11$pDgWTkRfzZfrKuxJila8u.X6fzxr1K.PFbdkgQrEl5vGHqovzlr1e"; // Supervisor@123
        const string agentPasswordHash = "$2a$11$CTCCNGRvH2t2mMI7eW0KCOm0w7RUq3mm/6WTulJ22y0NBz38mlez6"; // Agent@123

        // 1. Seed SuperAdmin user (no project required)
        var superAdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = superAdminId,
            Email = "admin@campaignmanager.local",
            PasswordHash = adminPasswordHash,
            FirstName = "Super",
            LastName = "Admin",
            Role = UserRole.SuperAdmin,
            IsActive = true,
            CreatedAt = seedDate
        });

        // 2. Seed a sample Project
        var sampleProjectId = Guid.Parse("00000000-0000-0000-0000-000000000010");
        modelBuilder.Entity<Project>().HasData(new Project
        {
            Id = sampleProjectId,
            Name = "Demo Project",
            Description = "A sample project for demonstration purposes",
            IsActive = true,
            CreatedAt = seedDate
        });

        // 3. Seed License for the sample project
        var sampleLicenseId = Guid.Parse("00000000-0000-0000-0000-000000000011");
        modelBuilder.Entity<License>().HasData(new License
        {
            Id = sampleLicenseId,
            ProjectId = sampleProjectId,
            LicenseKey = "DEMO-1234-5678-ABCD",
            MaxUsers = 50,
            MaxCampaigns = 10,
            ExpiryDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            IsActivated = true,
            ActivatedAt = seedDate,
            CreatedAt = seedDate
        });

        // 4. Seed SubAdmin for the sample project
        var subAdminId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = subAdminId,
            ProjectId = sampleProjectId,
            Email = "subadmin@demo.local",
            PasswordHash = subAdminPasswordHash,
            FirstName = "Sub",
            LastName = "Admin",
            Role = UserRole.SubAdmin,
            IsActive = true,
            CreatedAt = seedDate
        });

        // 5. Seed Supervisor for the sample project
        var supervisorId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = supervisorId,
            ProjectId = sampleProjectId,
            Email = "supervisor@demo.local",
            PasswordHash = supervisorPasswordHash,
            FirstName = "Demo",
            LastName = "Supervisor",
            Role = UserRole.Supervisor,
            IsActive = true,
            CreatedAt = seedDate
        });

        // 6. Seed Agent for the sample project (reports to supervisor)
        var agentId = Guid.Parse("00000000-0000-0000-0000-000000000004");
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = agentId,
            ProjectId = sampleProjectId,
            Email = "agent@demo.local",
            PasswordHash = agentPasswordHash,
            FirstName = "Demo",
            LastName = "Agent",
            Role = UserRole.Agent,
            SupervisorId = supervisorId,
            IsActive = true,
            CreatedAt = seedDate
        });

        // 7. Seed sample dispositions for the project
        var dispositionQualifiedId = Guid.Parse("00000000-0000-0000-0000-000000000020");
        var dispositionNotInterestedId = Guid.Parse("00000000-0000-0000-0000-000000000021");
        var dispositionCallbackId = Guid.Parse("00000000-0000-0000-0000-000000000022");
        var dispositionNoAnswerId = Guid.Parse("00000000-0000-0000-0000-000000000023");

        modelBuilder.Entity<Disposition>().HasData(
            new Disposition
            {
                Id = dispositionQualifiedId,
                ProjectId = sampleProjectId,
                Code = "QUALIFIED",
                Name = "Qualified Lead",
                IsQualified = true,
                RequiresCallback = false,
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = seedDate
            },
            new Disposition
            {
                Id = dispositionNotInterestedId,
                ProjectId = sampleProjectId,
                Code = "NOT_INTERESTED",
                Name = "Not Interested",
                IsQualified = false,
                RequiresCallback = false,
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = seedDate
            },
            new Disposition
            {
                Id = dispositionCallbackId,
                ProjectId = sampleProjectId,
                Code = "CALLBACK",
                Name = "Callback Required",
                IsQualified = false,
                RequiresCallback = true,
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = seedDate
            },
            new Disposition
            {
                Id = dispositionNoAnswerId,
                ProjectId = sampleProjectId,
                Code = "NO_ANSWER",
                Name = "No Answer",
                IsQualified = false,
                RequiresCallback = true,
                DisplayOrder = 4,
                IsActive = true,
                CreatedAt = seedDate
            }
        );

        // 8. Seed a sample Campaign
        var sampleCampaignId = Guid.Parse("00000000-0000-0000-0000-000000000030");
        modelBuilder.Entity<Campaign>().HasData(new Campaign
        {
            Id = sampleCampaignId,
            ProjectId = sampleProjectId,
            Name = "Demo Email Campaign",
            Description = "A sample email campaign for demonstration",
            Channel = ChannelType.Email,
            Status = CampaignStatus.Draft,
            SchedulerEnabled = false,
            CreatedById = subAdminId,
            CreatedAt = seedDate
        });

        // 9. Seed campaign fields
        modelBuilder.Entity<CampaignField>().HasData(
            new CampaignField
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000040"),
                CampaignId = sampleCampaignId,
                FieldName = "firstName",
                DisplayName = "First Name",
                FieldType = FieldType.String,
                IsRequired = true,
                DisplayOrder = 1,
                CreatedAt = seedDate
            },
            new CampaignField
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000041"),
                CampaignId = sampleCampaignId,
                FieldName = "lastName",
                DisplayName = "Last Name",
                FieldType = FieldType.String,
                IsRequired = true,
                DisplayOrder = 2,
                CreatedAt = seedDate
            },
            new CampaignField
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000042"),
                CampaignId = sampleCampaignId,
                FieldName = "email",
                DisplayName = "Email Address",
                FieldType = FieldType.Email,
                IsRequired = true,
                DisplayOrder = 3,
                CreatedAt = seedDate
            },
            new CampaignField
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000043"),
                CampaignId = sampleCampaignId,
                FieldName = "phone",
                DisplayName = "Phone Number",
                FieldType = FieldType.Phone,
                IsRequired = false,
                DisplayOrder = 4,
                CreatedAt = seedDate
            }
        );

        // 10. Seed campaign API config
        modelBuilder.Entity<CampaignApiConfig>().HasData(new CampaignApiConfig
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000050"),
            CampaignId = sampleCampaignId,
            SendGridFromEmail = "noreply@demo.local",
            SendGridFromName = "Demo Campaign",
            CreatedAt = seedDate
        });

        // 11. Seed campaign strategy
        modelBuilder.Entity<CampaignStrategy>().HasData(new CampaignStrategy
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000051"),
            CampaignId = sampleCampaignId,
            MaxEmailAttempts = 3,
            MaxCallAttempts = 5,
            EmailRetryIntervalMinutes = 1440, // 24 hours
            CallRetryIntervalMinutes = 60,
            WorkingDays = "Mon,Tue,Wed,Thu,Fri",
            CreatedAt = seedDate
        });
    }
}
