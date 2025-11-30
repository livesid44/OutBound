using System.ComponentModel.DataAnnotations;

namespace CampaignManager.Shared.Models;

/// <summary>
/// Represents a tenant/project in the multi-tenant system
/// </summary>
public class Project
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual License? License { get; set; }
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
}
