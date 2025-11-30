using System.Text.Json;
using CampaignManager.Data.Repositories;
using CampaignManager.Shared.DTOs;
using CampaignManager.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CampaignManager.Api.Services;

public interface ICampaignService
{
    Task<CampaignDto?> GetByIdAsync(Guid id);
    Task<PagedResult<CampaignDto>> GetCampaignsAsync(Guid projectId, int page, int pageSize);
    Task<ApiResponse<CampaignDto>> CreateCampaignAsync(Guid projectId, Guid userId, CreateCampaignRequest request);
    Task<ApiResponse<CampaignDto>> UpdateCampaignAsync(Guid id, UpdateCampaignRequest request);
    Task<ApiResponse<bool>> DeleteCampaignAsync(Guid id);
    
    // Campaign Fields
    Task<List<CampaignFieldDto>> GetFieldsAsync(Guid campaignId);
    Task<ApiResponse<CampaignFieldDto>> AddFieldAsync(Guid campaignId, CreateCampaignFieldRequest request);
    Task<ApiResponse<bool>> DeleteFieldAsync(Guid fieldId);
    
    // API Config
    Task<CampaignApiConfigDto?> GetApiConfigAsync(Guid campaignId);
    Task<ApiResponse<CampaignApiConfigDto>> UpdateApiConfigAsync(Guid campaignId, UpdateApiConfigRequest request);
    
    // Strategy
    Task<CampaignStrategyDto?> GetStrategyAsync(Guid campaignId);
    Task<ApiResponse<CampaignStrategyDto>> UpdateStrategyAsync(Guid campaignId, UpdateStrategyRequest request);
    
    // Scheduler
    Task<ApiResponse<bool>> ToggleSchedulerAsync(Guid campaignId, bool enable);
}

public class CampaignService : ICampaignService
{
    private readonly IUnitOfWork _unitOfWork;

    public CampaignService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CampaignDto?> GetByIdAsync(Guid id)
    {
        var campaign = await _unitOfWork.Repository<Campaign>()
            .Query()
            .Include(c => c.CreatedBy)
            .Include(c => c.ApiConfig)
            .Include(c => c.Strategy)
            .Include(c => c.Fields.OrderBy(f => f.DisplayOrder))
            .Include(c => c.Leads)
            .FirstOrDefaultAsync(c => c.Id == id);

        return campaign != null ? MapToDto(campaign) : null;
    }

    public async Task<PagedResult<CampaignDto>> GetCampaignsAsync(Guid projectId, int page, int pageSize)
    {
        var query = _unitOfWork.Repository<Campaign>()
            .Query()
            .Where(c => c.ProjectId == projectId);

        var totalCount = await query.CountAsync();
        var campaigns = await query
            .Include(c => c.CreatedBy)
            .Include(c => c.Leads)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<CampaignDto>
        {
            Items = campaigns.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<ApiResponse<CampaignDto>> CreateCampaignAsync(Guid projectId, Guid userId, CreateCampaignRequest request)
    {
        // Check license limits
        var license = await _unitOfWork.Repository<License>()
            .FirstOrDefaultAsync(l => l.ProjectId == projectId);

        if (license != null)
        {
            var currentCount = await _unitOfWork.Repository<Campaign>()
                .CountAsync(c => c.ProjectId == projectId);

            if (currentCount >= license.MaxCampaigns)
            {
                return ApiResponse<CampaignDto>.ErrorResponse("Maximum campaign limit reached for this project");
            }
        }

        // Ensure at least one channel is selected, default to Email
        var channels = request.Channels != ChannelType.None ? request.Channels : ChannelType.Email;

        var campaign = new Campaign
        {
            ProjectId = projectId,
            Name = request.Name,
            Description = request.Description,
            Channels = channels,
            Status = CampaignStatus.Draft,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Campaign>().AddAsync(campaign);

        // Create default API config
        var apiConfig = new CampaignApiConfig
        {
            CampaignId = campaign.Id,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<CampaignApiConfig>().AddAsync(apiConfig);

        // Create default strategy
        var strategy = new CampaignStrategy
        {
            CampaignId = campaign.Id,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<CampaignStrategy>().AddAsync(strategy);

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<CampaignDto>.SuccessResponse(MapToDto(campaign));
    }

    public async Task<ApiResponse<CampaignDto>> UpdateCampaignAsync(Guid id, UpdateCampaignRequest request)
    {
        var campaign = await _unitOfWork.Repository<Campaign>()
            .Query()
            .Include(c => c.CreatedBy)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null)
        {
            return ApiResponse<CampaignDto>.ErrorResponse("Campaign not found");
        }

        if (request.Name != null) campaign.Name = request.Name;
        if (request.Description != null) campaign.Description = request.Description;
        if (request.Channels.HasValue) campaign.Channels = request.Channels.Value;
        if (request.Status.HasValue) campaign.Status = request.Status.Value;
        if (request.IsSchedulerEnabled.HasValue) campaign.IsSchedulerEnabled = request.IsSchedulerEnabled.Value;

        campaign.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Campaign>().Update(campaign);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<CampaignDto>.SuccessResponse(MapToDto(campaign));
    }

    public async Task<ApiResponse<bool>> DeleteCampaignAsync(Guid id)
    {
        var campaign = await _unitOfWork.Repository<Campaign>().GetByIdAsync(id);
        if (campaign == null)
        {
            return ApiResponse<bool>.ErrorResponse("Campaign not found");
        }

        _unitOfWork.Repository<Campaign>().Remove(campaign);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task<List<CampaignFieldDto>> GetFieldsAsync(Guid campaignId)
    {
        var fields = await _unitOfWork.Repository<CampaignField>()
            .Query()
            .Where(f => f.CampaignId == campaignId)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync();

        return fields.Select(MapFieldToDto).ToList();
    }

    public async Task<ApiResponse<CampaignFieldDto>> AddFieldAsync(Guid campaignId, CreateCampaignFieldRequest request)
    {
        var existingField = await _unitOfWork.Repository<CampaignField>()
            .FirstOrDefaultAsync(f => f.CampaignId == campaignId && f.FieldName == request.FieldName);

        if (existingField != null)
        {
            return ApiResponse<CampaignFieldDto>.ErrorResponse("Field with this name already exists");
        }

        var field = new CampaignField
        {
            CampaignId = campaignId,
            FieldName = request.FieldName,
            DisplayName = request.DisplayName,
            FieldType = request.FieldType,
            IsRequired = request.IsRequired,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<CampaignField>().AddAsync(field);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<CampaignFieldDto>.SuccessResponse(MapFieldToDto(field));
    }

    public async Task<ApiResponse<bool>> DeleteFieldAsync(Guid fieldId)
    {
        var field = await _unitOfWork.Repository<CampaignField>().GetByIdAsync(fieldId);
        if (field == null)
        {
            return ApiResponse<bool>.ErrorResponse("Field not found");
        }

        _unitOfWork.Repository<CampaignField>().Remove(field);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task<CampaignApiConfigDto?> GetApiConfigAsync(Guid campaignId)
    {
        var config = await _unitOfWork.Repository<CampaignApiConfig>()
            .FirstOrDefaultAsync(c => c.CampaignId == campaignId);

        return config != null ? MapApiConfigToDto(config) : null;
    }

    public async Task<ApiResponse<CampaignApiConfigDto>> UpdateApiConfigAsync(Guid campaignId, UpdateApiConfigRequest request)
    {
        var config = await _unitOfWork.Repository<CampaignApiConfig>()
            .FirstOrDefaultAsync(c => c.CampaignId == campaignId);

        if (config == null)
        {
            config = new CampaignApiConfig
            {
                CampaignId = campaignId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<CampaignApiConfig>().AddAsync(config);
        }

        if (request.SendGridApiKey != null) config.SendGridApiKey = request.SendGridApiKey;
        if (request.SendGridFromEmail != null) config.SendGridFromEmail = request.SendGridFromEmail;
        if (request.SendGridFromName != null) config.SendGridFromName = request.SendGridFromName;
        if (request.SendGridTemplateId != null) config.SendGridTemplateId = request.SendGridTemplateId;
        if (request.EmailTemplateHtml != null) config.EmailTemplateHtml = request.EmailTemplateHtml;
        if (request.EmailSubject != null) config.EmailSubject = request.EmailSubject;
        if (request.WebExCurl != null) config.WebExCurl = request.WebExCurl;
        if (request.WebExApiEndpoint != null) config.WebExApiEndpoint = request.WebExApiEndpoint;
        if (request.WebExAuthToken != null) config.WebExAuthToken = request.WebExAuthToken;
        if (request.ParameterMappings != null) config.ParameterMappings = request.ParameterMappings;
        if (request.TokenGenerationConfig != null) config.TokenGenerationConfig = request.TokenGenerationConfig;

        config.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<CampaignApiConfigDto>.SuccessResponse(MapApiConfigToDto(config));
    }

    public async Task<CampaignStrategyDto?> GetStrategyAsync(Guid campaignId)
    {
        var strategy = await _unitOfWork.Repository<CampaignStrategy>()
            .FirstOrDefaultAsync(s => s.CampaignId == campaignId);

        return strategy != null ? MapStrategyToDto(strategy) : null;
    }

    public async Task<ApiResponse<CampaignStrategyDto>> UpdateStrategyAsync(Guid campaignId, UpdateStrategyRequest request)
    {
        var strategy = await _unitOfWork.Repository<CampaignStrategy>()
            .FirstOrDefaultAsync(s => s.CampaignId == campaignId);

        if (strategy == null)
        {
            strategy = new CampaignStrategy
            {
                CampaignId = campaignId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<CampaignStrategy>().AddAsync(strategy);
        }

        if (request.MaxEmailAttempts.HasValue) strategy.MaxEmailAttempts = request.MaxEmailAttempts.Value;
        if (request.MaxCallAttempts.HasValue) strategy.MaxCallAttempts = request.MaxCallAttempts.Value;
        if (request.EmailRetryIntervalMinutes.HasValue) strategy.EmailRetryIntervalMinutes = request.EmailRetryIntervalMinutes.Value;
        if (request.CallRetryIntervalMinutes.HasValue) strategy.CallRetryIntervalMinutes = request.CallRetryIntervalMinutes.Value;
        if (request.CallStartTime.HasValue) strategy.CallStartTime = request.CallStartTime.Value;
        if (request.CallEndTime.HasValue) strategy.CallEndTime = request.CallEndTime.Value;
        if (request.WorkingDays != null) strategy.WorkingDays = request.WorkingDays;
        if (request.StrategyRulesJson != null) strategy.StrategyRulesJson = request.StrategyRulesJson;

        strategy.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<CampaignStrategyDto>.SuccessResponse(MapStrategyToDto(strategy));
    }

    public async Task<ApiResponse<bool>> ToggleSchedulerAsync(Guid campaignId, bool enable)
    {
        var campaign = await _unitOfWork.Repository<Campaign>().GetByIdAsync(campaignId);
        if (campaign == null)
        {
            return ApiResponse<bool>.ErrorResponse("Campaign not found");
        }

        campaign.IsSchedulerEnabled = enable;
        campaign.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Campaign>().Update(campaign);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, enable ? "Scheduler enabled" : "Scheduler disabled");
    }

    private static CampaignDto MapToDto(Campaign campaign)
    {
        return new CampaignDto
        {
            Id = campaign.Id,
            ProjectId = campaign.ProjectId,
            Name = campaign.Name,
            Description = campaign.Description,
            Channels = campaign.Channels,
            Status = campaign.Status,
            IsSchedulerEnabled = campaign.IsSchedulerEnabled,
            LastProcessedAt = campaign.LastProcessedAt,
            NextScheduledAt = campaign.NextScheduledAt,
            CreatedAt = campaign.CreatedAt,
            CreatedByName = campaign.CreatedBy != null ? $"{campaign.CreatedBy.FirstName} {campaign.CreatedBy.LastName}" : null,
            LeadCount = campaign.Leads?.Count ?? 0,
            ApiConfig = campaign.ApiConfig != null ? MapApiConfigToDto(campaign.ApiConfig) : null,
            Strategy = campaign.Strategy != null ? MapStrategyToDto(campaign.Strategy) : null,
            Fields = campaign.Fields?.Select(MapFieldToDto).ToList() ?? new List<CampaignFieldDto>()
        };
    }

    private static CampaignFieldDto MapFieldToDto(CampaignField field)
    {
        return new CampaignFieldDto
        {
            Id = field.Id,
            FieldName = field.FieldName,
            DisplayName = field.DisplayName,
            FieldType = field.FieldType,
            IsRequired = field.IsRequired,
            DisplayOrder = field.DisplayOrder
        };
    }

    private static CampaignApiConfigDto MapApiConfigToDto(CampaignApiConfig config)
    {
        return new CampaignApiConfigDto
        {
            Id = config.Id,
            SendGridApiKey = config.SendGridApiKey,
            SendGridFromEmail = config.SendGridFromEmail,
            SendGridFromName = config.SendGridFromName,
            SendGridTemplateId = config.SendGridTemplateId,
            EmailTemplateHtml = config.EmailTemplateHtml,
            EmailSubject = config.EmailSubject,
            WebExCurl = config.WebExCurl,
            WebExApiEndpoint = config.WebExApiEndpoint,
            WebExAuthToken = config.WebExAuthToken,
            ParameterMappings = config.ParameterMappings,
            TokenGenerationConfig = config.TokenGenerationConfig
        };
    }

    private static CampaignStrategyDto MapStrategyToDto(CampaignStrategy strategy)
    {
        return new CampaignStrategyDto
        {
            Id = strategy.Id,
            MaxEmailAttempts = strategy.MaxEmailAttempts,
            MaxCallAttempts = strategy.MaxCallAttempts,
            EmailRetryIntervalMinutes = strategy.EmailRetryIntervalMinutes,
            CallRetryIntervalMinutes = strategy.CallRetryIntervalMinutes,
            CallStartTime = strategy.CallStartTime,
            CallEndTime = strategy.CallEndTime,
            WorkingDays = strategy.WorkingDays,
            StrategyRulesJson = strategy.StrategyRulesJson
        };
    }
}
