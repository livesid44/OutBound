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

    // Voice Blast configuration
    [MaxLength(500)]
    public string? VoiceBlastApiEndpoint { get; set; }

    [MaxLength(256)]
    public string? VoiceBlastApiKey { get; set; }

    [MaxLength(100)]
    public string? VoiceBlastCampaignId { get; set; }

    /// <summary>
    /// Voice message script or audio file URL
    /// </summary>
    public string? VoiceBlastMessage { get; set; }

    /// <summary>
    /// Voice Blast parameter mappings stored as JSON string
    /// </summary>
    public string? VoiceBlastParameterMappings { get; set; }

    // WhatsApp configuration (Twilio or Meta Business API)
    [MaxLength(500)]
    public string? WhatsAppApiEndpoint { get; set; }

    [MaxLength(256)]
    public string? WhatsAppApiKey { get; set; }

    [MaxLength(100)]
    public string? WhatsAppAccountSid { get; set; }

    [MaxLength(100)]
    public string? WhatsAppAuthToken { get; set; }

    [MaxLength(50)]
    public string? WhatsAppFromNumber { get; set; }

    /// <summary>
    /// WhatsApp message template content with dynamic field placeholders
    /// </summary>
    public string? WhatsAppTemplate { get; set; }

    /// <summary>
    /// WhatsApp template name (for pre-approved templates)
    /// </summary>
    [MaxLength(200)]
    public string? WhatsAppTemplateName { get; set; }

    // SMS configuration (Twilio or similar)
    [MaxLength(500)]
    public string? SmsApiEndpoint { get; set; }

    [MaxLength(256)]
    public string? SmsApiKey { get; set; }

    [MaxLength(100)]
    public string? SmsAccountSid { get; set; }

    [MaxLength(100)]
    public string? SmsAuthToken { get; set; }

    [MaxLength(50)]
    public string? SmsFromNumber { get; set; }

    /// <summary>
    /// SMS message template content with dynamic field placeholders
    /// </summary>
    [MaxLength(1600)]
    public string? SmsTemplate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    [ForeignKey(nameof(CampaignId))]
    public virtual Campaign? Campaign { get; set; }
}
