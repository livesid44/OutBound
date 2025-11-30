using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampaignManager.Shared.Models;

/// <summary>
/// Lead status in the campaign
/// </summary>
public enum LeadStatus
{
    Queued = 0,
    EmailPending = 1,
    EmailSent = 2,
    CallPending = 3,
    Dialed = 4,
    Connected = 5,
    RightPartyContact = 6,
    Disposed = 7,
    Failed = 8,
    NoAnswer = 9,
    Busy = 10,
    Voicemail = 11
}

/// <summary>
/// Email-specific lead status
/// </summary>
public enum LeadEmailStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    Bounced = 3,
    Opened = 4,
    Clicked = 5
}

/// <summary>
/// Call-specific lead status
/// </summary>
public enum LeadCallStatus
{
    Pending = 0,
    Dialed = 1,
    Connected = 2,
    RightPartyContact = 3,
    NoAnswer = 4,
    Busy = 5,
    Voicemail = 6,
    Failed = 7,
    Disposed = 8
}

/// <summary>
/// Represents a lead/contact in a campaign
/// </summary>
public class CampaignLead
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CampaignId { get; set; }

    // Custom field data stored as JSON
    public string? Data { get; set; }

    public LeadStatus Status { get; set; } = LeadStatus.Queued;

    // Email tracking
    public LeadEmailStatus EmailStatus { get; set; } = LeadEmailStatus.Pending;
    public int EmailAttempts { get; set; } = 0;
    public DateTime? LastEmailAttemptAt { get; set; }
    public DateTime? EmailSentAt { get; set; }

    // Call tracking
    public LeadCallStatus CallStatus { get; set; } = LeadCallStatus.Pending;
    public int CallAttempts { get; set; } = 0;
    public DateTime? LastCallAttemptAt { get; set; }
    public DateTime? CallConnectedAt { get; set; }

    public DateTime? NextScheduledAction { get; set; }

    public Guid? AssignedAgentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(CampaignId))]
    public virtual Campaign? Campaign { get; set; }

    [ForeignKey(nameof(AssignedAgentId))]
    public virtual User? AssignedAgent { get; set; }

    public virtual LeadDisposition? Disposition { get; set; }
}
