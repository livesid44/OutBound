using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampaignManager.Shared.Models;

/// <summary>
/// Represents a license for a project with user and campaign limits
/// </summary>
public class License
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    [MaxLength(100)]
    public string LicenseKey { get; set; } = string.Empty;

    public int MaxUsers { get; set; }

    public int MaxCampaigns { get; set; }

    public DateTime ExpiryDate { get; set; }

    public bool IsActivated { get; set; } = false;

    public DateTime? ActivatedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }
}
