using CampaignManager.Shared.Models;

namespace CampaignManager.Shared.DTOs;

public class LeadDto
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public string? Data { get; set; }
    public Dictionary<string, object?>? ParsedData { get; set; }
    public LeadStatus Status { get; set; }
    
    // Email tracking
    public LeadEmailStatus EmailStatus { get; set; }
    public int EmailAttempts { get; set; }
    public DateTime? LastEmailAttemptAt { get; set; }
    public DateTime? EmailSentAt { get; set; }
    
    // Call tracking
    public LeadCallStatus CallStatus { get; set; }
    public int CallAttempts { get; set; }
    public DateTime? LastCallAttemptAt { get; set; }
    public DateTime? CallConnectedAt { get; set; }
    
    public DateTime? NextScheduledAction { get; set; }
    public Guid? AssignedAgentId { get; set; }
    public string? AssignedAgentName { get; set; }
    public DateTime CreatedAt { get; set; }
    public LeadDispositionDto? Disposition { get; set; }
}

public class CreateLeadRequest
{
    public Dictionary<string, object?> Data { get; set; } = new();
}

public class BulkLeadUploadRequest
{
    public List<Dictionary<string, object?>> Leads { get; set; } = new();
}

public class BulkLeadUploadResult
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class LeadDispositionDto
{
    public Guid Id { get; set; }
    public string DispositionCode { get; set; } = string.Empty;
    public string? SubDispositionCode { get; set; }
    public DateTime? CallbackDateTime { get; set; }
    public string? Comments { get; set; }
    public string? DisposedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDispositionRequest
{
    public string DispositionCode { get; set; } = string.Empty;
    public string? SubDispositionCode { get; set; }
    public DateTime? CallbackDateTime { get; set; }
    public string? Comments { get; set; }
}

public class DispositionCodeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsQualified { get; set; }
    public bool RequiresCallback { get; set; }
    public int DisplayOrder { get; set; }
    public List<DispositionCodeDto> SubDispositions { get; set; } = new();
}
