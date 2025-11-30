using System.Text.Json;
using CampaignManager.Data.Repositories;
using CampaignManager.Shared.DTOs;
using CampaignManager.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CampaignManager.Api.Services;

public interface ILeadService
{
    Task<LeadDto?> GetByIdAsync(Guid id);
    Task<PagedResult<LeadDto>> GetLeadsAsync(Guid campaignId, LeadStatus? status, int page, int pageSize);
    Task<ApiResponse<LeadDto>> CreateLeadAsync(Guid campaignId, CreateLeadRequest request);
    Task<BulkLeadUploadResult> BulkUploadLeadsAsync(Guid campaignId, BulkLeadUploadRequest request);
    Task<ApiResponse<LeadDto>> UpdateLeadStatusAsync(Guid leadId, LeadStatus status);
    Task<ApiResponse<LeadDto>> DisposeLeadAsync(Guid leadId, Guid agentId, CreateDispositionRequest request);
    Task<CampaignStatsDto> GetCampaignStatsAsync(Guid campaignId);
    Task<QueueStatusDto> GetQueueStatusAsync(Guid campaignId);
}

public class LeadService : ILeadService
{
    private readonly IUnitOfWork _unitOfWork;

    public LeadService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<LeadDto?> GetByIdAsync(Guid id)
    {
        var lead = await _unitOfWork.Repository<CampaignLead>()
            .Query()
            .Include(l => l.AssignedAgent)
            .Include(l => l.Disposition)
            .ThenInclude(d => d!.DisposedBy)
            .FirstOrDefaultAsync(l => l.Id == id);

        return lead != null ? MapToDto(lead) : null;
    }

    public async Task<PagedResult<LeadDto>> GetLeadsAsync(Guid campaignId, LeadStatus? status, int page, int pageSize)
    {
        var query = _unitOfWork.Repository<CampaignLead>()
            .Query()
            .Where(l => l.CampaignId == campaignId);

        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        var totalCount = await query.CountAsync();
        var leads = await query
            .Include(l => l.AssignedAgent)
            .Include(l => l.Disposition)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<LeadDto>
        {
            Items = leads.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<ApiResponse<LeadDto>> CreateLeadAsync(Guid campaignId, CreateLeadRequest request)
    {
        var lead = new CampaignLead
        {
            CampaignId = campaignId,
            FieldData = JsonSerializer.Serialize(request.FieldData),
            Status = LeadStatus.Queued,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<CampaignLead>().AddAsync(lead);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<LeadDto>.SuccessResponse(MapToDto(lead));
    }

    public async Task<BulkLeadUploadResult> BulkUploadLeadsAsync(Guid campaignId, BulkLeadUploadRequest request)
    {
        var result = new BulkLeadUploadResult
        {
            TotalCount = request.Leads.Count
        };

        var leads = new List<CampaignLead>();

        foreach (var leadData in request.Leads)
        {
            try
            {
                var lead = new CampaignLead
                {
                    CampaignId = campaignId,
                    FieldData = JsonSerializer.Serialize(leadData),
                    Status = LeadStatus.Queued,
                    CreatedAt = DateTime.UtcNow
                };

                leads.Add(lead);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.Errors.Add($"Error processing lead: {ex.Message}");
            }
        }

        await _unitOfWork.Repository<CampaignLead>().AddRangeAsync(leads);
        await _unitOfWork.SaveChangesAsync();

        return result;
    }

    public async Task<ApiResponse<LeadDto>> UpdateLeadStatusAsync(Guid leadId, LeadStatus status)
    {
        var lead = await _unitOfWork.Repository<CampaignLead>().GetByIdAsync(leadId);
        if (lead == null)
        {
            return ApiResponse<LeadDto>.ErrorResponse("Lead not found");
        }

        lead.Status = status;
        lead.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<CampaignLead>().Update(lead);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<LeadDto>.SuccessResponse(MapToDto(lead));
    }

    public async Task<ApiResponse<LeadDto>> DisposeLeadAsync(Guid leadId, Guid agentId, CreateDispositionRequest request)
    {
        var lead = await _unitOfWork.Repository<CampaignLead>()
            .Query()
            .Include(l => l.Disposition)
            .FirstOrDefaultAsync(l => l.Id == leadId);

        if (lead == null)
        {
            return ApiResponse<LeadDto>.ErrorResponse("Lead not found");
        }

        // Create or update disposition
        if (lead.Disposition != null)
        {
            lead.Disposition.DispositionCode = request.DispositionCode;
            lead.Disposition.SubDispositionCode = request.SubDispositionCode;
            lead.Disposition.CallbackDateTime = request.CallbackDateTime;
            lead.Disposition.Comments = request.Comments;
            lead.Disposition.DisposedById = agentId;
        }
        else
        {
            var disposition = new LeadDisposition
            {
                LeadId = leadId,
                DispositionCode = request.DispositionCode,
                SubDispositionCode = request.SubDispositionCode,
                CallbackDateTime = request.CallbackDateTime,
                Comments = request.Comments,
                DisposedById = agentId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<LeadDisposition>().AddAsync(disposition);
        }

        lead.Status = LeadStatus.Disposed;
        lead.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<LeadDto>.SuccessResponse(MapToDto(lead));
    }

    public async Task<CampaignStatsDto> GetCampaignStatsAsync(Guid campaignId)
    {
        var campaign = await _unitOfWork.Repository<Campaign>()
            .Query()
            .Include(c => c.Leads)
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign == null)
        {
            return new CampaignStatsDto { CampaignId = campaignId };
        }

        var leads = campaign.Leads ?? new List<CampaignLead>();

        return new CampaignStatsDto
        {
            CampaignId = campaign.Id,
            CampaignName = campaign.Name,
            TotalLeads = leads.Count,
            QueuedCount = leads.Count(l => l.Status == LeadStatus.Queued),
            EmailPendingCount = leads.Count(l => l.Status == LeadStatus.EmailPending),
            EmailSentCount = leads.Count(l => l.Status == LeadStatus.EmailSent),
            CallPendingCount = leads.Count(l => l.Status == LeadStatus.CallPending),
            DialedCount = leads.Count(l => l.Status == LeadStatus.Dialed),
            ConnectedCount = leads.Count(l => l.Status == LeadStatus.Connected),
            RightPartyContactCount = leads.Count(l => l.Status == LeadStatus.RightPartyContact),
            DisposedCount = leads.Count(l => l.Status == LeadStatus.Disposed),
            FailedCount = leads.Count(l => l.Status == LeadStatus.Failed)
        };
    }

    public async Task<QueueStatusDto> GetQueueStatusAsync(Guid campaignId)
    {
        var leads = await _unitOfWork.Repository<CampaignLead>()
            .Query()
            .Where(l => l.CampaignId == campaignId)
            .ToListAsync();

        return new QueueStatusDto
        {
            EmailQueueCount = leads.Count(l => l.Status == LeadStatus.EmailPending),
            VoiceQueueCount = leads.Count(l => l.Status == LeadStatus.CallPending),
            LastUpdated = DateTime.UtcNow
        };
    }

    private static LeadDto MapToDto(CampaignLead lead)
    {
        Dictionary<string, object?>? parsedData = null;
        if (!string.IsNullOrEmpty(lead.FieldData))
        {
            try
            {
                parsedData = JsonSerializer.Deserialize<Dictionary<string, object?>>(lead.FieldData);
            }
            catch
            {
                // Ignore parsing errors
            }
        }

        return new LeadDto
        {
            Id = lead.Id,
            CampaignId = lead.CampaignId,
            FieldData = lead.FieldData,
            ParsedFieldData = parsedData,
            Status = lead.Status,
            EmailAttempts = lead.EmailAttempts,
            CallAttempts = lead.CallAttempts,
            LastEmailAttempt = lead.LastEmailAttempt,
            LastCallAttempt = lead.LastCallAttempt,
            NextScheduledAction = lead.NextScheduledAction,
            AssignedAgentId = lead.AssignedAgentId,
            AssignedAgentName = lead.AssignedAgent != null ? $"{lead.AssignedAgent.FirstName} {lead.AssignedAgent.LastName}" : null,
            CreatedAt = lead.CreatedAt,
            Disposition = lead.Disposition != null ? MapDispositionToDto(lead.Disposition) : null
        };
    }

    private static LeadDispositionDto MapDispositionToDto(LeadDisposition disposition)
    {
        return new LeadDispositionDto
        {
            Id = disposition.Id,
            DispositionCode = disposition.DispositionCode,
            SubDispositionCode = disposition.SubDispositionCode,
            CallbackDateTime = disposition.CallbackDateTime,
            Comments = disposition.Comments,
            DisposedByName = disposition.DisposedBy != null ? $"{disposition.DisposedBy.FirstName} {disposition.DisposedBy.LastName}" : null,
            CreatedAt = disposition.CreatedAt
        };
    }
}
