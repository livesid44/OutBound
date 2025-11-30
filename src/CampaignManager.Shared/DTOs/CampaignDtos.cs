using CampaignManager.Shared.Models;

namespace CampaignManager.Shared.DTOs;

public class CampaignDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    /// <summary>
    /// Multiple channels enabled for this campaign (flags enum)
    /// </summary>
    public ChannelType Channels { get; set; }
    /// <summary>
    /// Legacy single channel property for backward compatibility
    /// </summary>
    public ChannelType Channel { get => Channels; set => Channels = value; }
    public CampaignStatus Status { get; set; }
    public bool IsSchedulerEnabled { get; set; }
    public DateTime? LastProcessedAt { get; set; }
    public DateTime? NextScheduledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByName { get; set; }
    public int LeadCount { get; set; }
    public CampaignApiConfigDto? ApiConfig { get; set; }
    public CampaignStrategyDto? Strategy { get; set; }
    public List<CampaignFieldDto> Fields { get; set; } = new();
}

public class CreateCampaignRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    /// <summary>
    /// Multiple channels to enable (can combine: Email | Call)
    /// </summary>
    public ChannelType Channels { get; set; } = ChannelType.Email;
    /// <summary>
    /// Legacy single channel property for backward compatibility
    /// </summary>
    public ChannelType Channel { get => Channels; set => Channels = value; }
}

public class UpdateCampaignRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ChannelType? Channels { get; set; }
    public ChannelType? Channel { get => Channels; set => Channels = value; }
    public CampaignStatus? Status { get; set; }
    public bool? IsSchedulerEnabled { get; set; }
}

public class CampaignFieldDto
{
    public Guid Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public FieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
}

public class CreateCampaignFieldRequest
{
    public string FieldName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public FieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
}

public class CampaignApiConfigDto
{
    public Guid Id { get; set; }
    public string? SendGridApiKey { get; set; }
    public string? SendGridFromEmail { get; set; }
    public string? SendGridFromName { get; set; }
    public string? SendGridTemplateId { get; set; }
    public string? EmailTemplateHtml { get; set; }
    public string? EmailSubject { get; set; }
    public string? WebExCurl { get; set; }
    public string? WebExApiEndpoint { get; set; }
    public string? WebExAuthToken { get; set; }
    public string? ParameterMappings { get; set; }
    public string? TokenGenerationConfig { get; set; }
}

public class UpdateApiConfigRequest
{
    public string? SendGridApiKey { get; set; }
    public string? SendGridFromEmail { get; set; }
    public string? SendGridFromName { get; set; }
    public string? SendGridTemplateId { get; set; }
    public string? EmailTemplateHtml { get; set; }
    public string? EmailSubject { get; set; }
    public string? WebExCurl { get; set; }
    public string? WebExApiEndpoint { get; set; }
    public string? WebExAuthToken { get; set; }
    public string? ParameterMappings { get; set; }
    public string? TokenGenerationConfig { get; set; }
}

public class CampaignStrategyDto
{
    public Guid Id { get; set; }
    public int MaxEmailAttempts { get; set; }
    public int MaxCallAttempts { get; set; }
    public int EmailRetryIntervalMinutes { get; set; }
    public int CallRetryIntervalMinutes { get; set; }
    public TimeSpan? CallStartTime { get; set; }
    public TimeSpan? CallEndTime { get; set; }
    public string? WorkingDays { get; set; }
    public string? StrategyRulesJson { get; set; }
}

public class UpdateStrategyRequest
{
    public int? MaxEmailAttempts { get; set; }
    public int? MaxCallAttempts { get; set; }
    public int? EmailRetryIntervalMinutes { get; set; }
    public int? CallRetryIntervalMinutes { get; set; }
    public TimeSpan? CallStartTime { get; set; }
    public TimeSpan? CallEndTime { get; set; }
    public string? WorkingDays { get; set; }
    public string? StrategyRulesJson { get; set; }
}
