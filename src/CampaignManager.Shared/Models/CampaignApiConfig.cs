using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampaignManager.Shared.Models;

/// <summary>
/// Represents API configuration for a campaign (SendGrid, WebEx, etc.)
/// </summary>
public class CampaignApiConfig
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CampaignId { get; set; }

    // SendGrid configuration
    [MaxLength(256)]
    public string? SendGridApiKey { get; set; }

    [MaxLength(200)]
    public string? SendGridFromEmail { get; set; }

    [MaxLength(200)]
    public string? SendGridFromName { get; set; }

    [MaxLength(100)]
    public string? SendGridTemplateId { get; set; }

    /// <summary>
    /// Custom HTML email template content
    /// </summary>
    public string? EmailTemplateHtml { get; set; }

    /// <summary>
    /// Email subject line with dynamic field placeholders
    /// </summary>
    [MaxLength(500)]
    public string? EmailSubject { get; set; }

    // WebEx/Voice configuration
    [MaxLength(2000)]
    public string? WebExCurl { get; set; }

    [MaxLength(500)]
    public string? WebExApiEndpoint { get; set; }

    [MaxLength(256)]
    public string? WebExAuthToken { get; set; }

    // JSON parameter mappings stored as JSON string
    public string? ParameterMappings { get; set; }

    // Token generation configuration
    public string? TokenGenerationConfig { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    [ForeignKey(nameof(CampaignId))]
    public virtual Campaign? Campaign { get; set; }
}
