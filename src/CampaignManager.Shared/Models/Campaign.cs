using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampaignManager.Shared.Models;

/// <summary>
/// Communication channel types for campaigns (can be combined as flags)
/// </summary>
[Flags]
public enum ChannelType
{
    None = 0,
    Email = 1,
    Call = 2,
    VoiceBot = 4,
    WhatsApp = 8,
    SMS = 16
}

/// <summary>
/// Campaign status
/// </summary>
public enum CampaignStatus
{
    Draft = 0,
    Active = 1,
    Paused = 2,
    Completed = 3,
    Archived = 4
}

/// <summary>
/// Represents a campaign in the system
/// </summary>
public class Campaign
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Channels enabled for this campaign (can be multiple)
    /// </summary>
    public ChannelType Channels { get; set; } = ChannelType.Email;

    /// <summary>
    /// Legacy single channel property for backward compatibility
    /// </summary>
    [NotMapped]
    public ChannelType Channel
    {
        get => Channels;
        set => Channels = value;
    }

    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;

    public bool SchedulerEnabled { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedById { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public virtual User? CreatedBy { get; set; }

    public virtual CampaignApiConfig? ApiConfig { get; set; }
    public virtual CampaignStrategy? Strategy { get; set; }
    public virtual ICollection<CampaignField> Fields { get; set; } = new List<CampaignField>();
    public virtual ICollection<CampaignLead> Leads { get; set; } = new List<CampaignLead>();
}
