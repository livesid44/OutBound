using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampaignManager.Shared.Models;

/// <summary>
/// Represents the strategy/scheduling configuration for a campaign
/// </summary>
public class CampaignStrategy
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CampaignId { get; set; }

    public int MaxEmailAttempts { get; set; } = 3;

    public int MaxCallAttempts { get; set; } = 3;

    public int MaxVoiceBlastAttempts { get; set; } = 3;

    public int MaxWhatsAppAttempts { get; set; } = 3;

    public int MaxSmsAttempts { get; set; } = 3;

    public int EmailRetryIntervalMinutes { get; set; } = 60;

    public int CallRetryIntervalMinutes { get; set; } = 30;

    public int VoiceBlastRetryIntervalMinutes { get; set; } = 30;

    public int WhatsAppRetryIntervalMinutes { get; set; } = 60;

    public int SmsRetryIntervalMinutes { get; set; } = 60;

    // Working hours for calling
    public TimeSpan? CallStartTime { get; set; }
    public TimeSpan? CallEndTime { get; set; }

    // Days of week for scheduling (stored as comma-separated: "Mon,Tue,Wed")
    [MaxLength(100)]
    public string? WorkingDays { get; set; }

    // Strategy rules stored as JSON
    public string? StrategyRulesJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    [ForeignKey(nameof(CampaignId))]
    public virtual Campaign? Campaign { get; set; }
}
