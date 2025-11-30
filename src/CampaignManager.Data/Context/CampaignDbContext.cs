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
                  .OnDelete(DeleteBehavior.SetNull);
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
            entity.HasOne(e => e.Disposition)
                  .WithOne(d => d.Lead)
                  .HasForeignKey<LeadDisposition>(d => d.LeadId)
                  .OnDelete(DeleteBehavior.Cascade);
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

        // Seed SuperAdmin user
        var superAdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = superAdminId,
            Email = "admin@campaignmanager.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            FirstName = "Super",
            LastName = "Admin",
            Role = UserRole.SuperAdmin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
    }
}
