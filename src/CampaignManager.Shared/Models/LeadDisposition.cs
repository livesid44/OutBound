using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampaignManager.Shared.Models;

/// <summary>
/// Represents the disposition/outcome of a lead interaction
/// </summary>
public class LeadDisposition
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid LeadId { get; set; }

    [Required]
    [MaxLength(100)]
    public string DispositionCode { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? SubDispositionCode { get; set; }

    public DateTime? CallbackDateTime { get; set; }

    [MaxLength(2000)]
    public string? Comments { get; set; }

    public Guid? DisposedById { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(LeadId))]
    public virtual CampaignLead? Lead { get; set; }

    [ForeignKey(nameof(DisposedById))]
    public virtual User? DisposedBy { get; set; }
}
