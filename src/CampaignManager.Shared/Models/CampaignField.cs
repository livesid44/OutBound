using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampaignManager.Shared.Models;

/// <summary>
/// Data types for campaign fields
/// </summary>
public enum FieldType
{
    String = 0,
    Number = 1,
    Date = 2,
    Boolean = 3,
    Email = 4,
    Phone = 5
}

/// <summary>
/// Represents a custom field definition for a campaign
/// </summary>
public class CampaignField
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CampaignId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FieldName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? DisplayName { get; set; }

    public FieldType FieldType { get; set; } = FieldType.String;

    public bool IsRequired { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey(nameof(CampaignId))]
    public virtual Campaign? Campaign { get; set; }
}
