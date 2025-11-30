using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampaignManager.Shared.Models;

/// <summary>
/// Represents a disposition code configuration
/// </summary>
public class Disposition
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? ProjectId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }

    public bool IsQualified { get; set; } = false;

    public bool RequiresCallback { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }

    [ForeignKey(nameof(ParentId))]
    public virtual Disposition? Parent { get; set; }

    public virtual ICollection<Disposition> SubDispositions { get; set; } = new List<Disposition>();
}
