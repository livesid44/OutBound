using System.Security.Claims;
using CampaignManager.Api.Services;
using CampaignManager.Data.Repositories;
using CampaignManager.Shared.DTOs;
using CampaignManager.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampaignManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CampaignsController : ControllerBase
{
    private readonly ICampaignService _campaignService;
    private readonly ILeadService _leadService;
    private readonly IUnitOfWork _unitOfWork;

    public CampaignsController(ICampaignService campaignService, ILeadService leadService, IUnitOfWork unitOfWork)
    {
        _campaignService = campaignService;
        _leadService = leadService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CampaignDto>> GetById(Guid id)
    {
        var campaign = await _campaignService.GetByIdAsync(id);
        if (campaign == null)
        {
            return NotFound();
        }
        return Ok(campaign);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CampaignDto>>> GetCampaigns([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var projectId = await GetCurrentProjectIdAsync();
        if (projectId == null)
        {
            return BadRequest("Project context required");
        }

        var result = await _campaignService.GetCampaignsAsync(projectId.Value, page, pageSize);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,SubAdmin,Supervisor")]
    public async Task<ActionResult<ApiResponse<CampaignDto>>> Create([FromBody] CreateCampaignRequest request)
    {
        var projectId = await GetCurrentProjectIdAsync();
        var userId = GetCurrentUserId();
        if (projectId == null || userId == null)
        {
            return BadRequest(ApiResponse<CampaignDto>.ErrorResponse("Project and user context required. SuperAdmin users should have at least one project in the system."));
        }

        var result = await _campaignService.CreateCampaignAsync(projectId.Value, userId.Value, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,SubAdmin,Supervisor")]
    public async Task<ActionResult<ApiResponse<CampaignDto>>> Update(Guid id, [FromBody] UpdateCampaignRequest request)
    {
        var result = await _campaignService.UpdateCampaignAsync(id, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,SubAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _campaignService.DeleteCampaignAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    // Campaign Fields
    [HttpGet("{id:guid}/fields")]
    public async Task<ActionResult<List<CampaignFieldDto>>> GetFields(Guid id)
    {
        var result = await _campaignService.GetFieldsAsync(id);
        return Ok(result);
    }

    [HttpPost("{id:guid}/fields")]
    [Authorize(Roles = "SuperAdmin,SubAdmin,Supervisor")]
    public async Task<ActionResult<ApiResponse<CampaignFieldDto>>> AddField(Guid id, [FromBody] CreateCampaignFieldRequest request)
    {
        var result = await _campaignService.AddFieldAsync(id, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpDelete("fields/{fieldId:guid}")]
    [Authorize(Roles = "SuperAdmin,SubAdmin,Supervisor")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteField(Guid fieldId)
    {
        var result = await _campaignService.DeleteFieldAsync(fieldId);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    // API Config
    [HttpGet("{id:guid}/api-config")]
    public async Task<ActionResult<CampaignApiConfigDto>> GetApiConfig(Guid id)
    {
        var result = await _campaignService.GetApiConfigAsync(id);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPut("{id:guid}/api-config")]
    [Authorize(Roles = "SuperAdmin,SubAdmin,Supervisor")]
    public async Task<ActionResult<ApiResponse<CampaignApiConfigDto>>> UpdateApiConfig(Guid id, [FromBody] UpdateApiConfigRequest request)
    {
        var result = await _campaignService.UpdateApiConfigAsync(id, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    // Strategy
    [HttpGet("{id:guid}/strategy")]
    public async Task<ActionResult<CampaignStrategyDto>> GetStrategy(Guid id)
    {
        var result = await _campaignService.GetStrategyAsync(id);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPut("{id:guid}/strategy")]
    [Authorize(Roles = "SuperAdmin,SubAdmin,Supervisor")]
    public async Task<ActionResult<ApiResponse<CampaignStrategyDto>>> UpdateStrategy(Guid id, [FromBody] UpdateStrategyRequest request)
    {
        var result = await _campaignService.UpdateStrategyAsync(id, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    // Scheduler
    [HttpPost("{id:guid}/scheduler/toggle")]
    [Authorize(Roles = "SuperAdmin,SubAdmin,Supervisor")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleScheduler(Guid id, [FromQuery] bool enable)
    {
        var result = await _campaignService.ToggleSchedulerAsync(id, enable);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    // Leads
    [HttpGet("{id:guid}/leads")]
    public async Task<ActionResult<PagedResult<LeadDto>>> GetLeads(
        Guid id,
        [FromQuery] LeadStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _leadService.GetLeadsAsync(id, status, page, pageSize);
        return Ok(result);
    }

    [HttpPost("{id:guid}/leads")]
    public async Task<ActionResult<ApiResponse<LeadDto>>> CreateLead(Guid id, [FromBody] CreateLeadRequest request)
    {
        var result = await _leadService.CreateLeadAsync(id, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("{id:guid}/leads/bulk")]
    public async Task<ActionResult<BulkLeadUploadResult>> BulkUploadLeads(Guid id, [FromBody] BulkLeadUploadRequest request)
    {
        var result = await _leadService.BulkUploadLeadsAsync(id, request);
        return Ok(result);
    }

    // Stats
    [HttpGet("{id:guid}/stats")]
    public async Task<ActionResult<CampaignStatsDto>> GetStats(Guid id)
    {
        var result = await _leadService.GetCampaignStatsAsync(id);
        return Ok(result);
    }

    [HttpGet("{id:guid}/queue-status")]
    public async Task<ActionResult<QueueStatusDto>> GetQueueStatus(Guid id)
    {
        var result = await _leadService.GetQueueStatusAsync(id);
        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }

    private Guid? GetCurrentProjectId()
    {
        var projectIdClaim = User.FindFirst("ProjectId");
        if (projectIdClaim != null && Guid.TryParse(projectIdClaim.Value, out var projectId))
        {
            return projectId;
        }
        return null;
    }

    /// <summary>
    /// Gets the current user's project ID. For SuperAdmin users,
    /// checks for X-Project-Id header for tenant selection.
    /// </summary>
    private async Task<Guid?> GetCurrentProjectIdAsync()
    {
        // For SuperAdmin, check for X-Project-Id header (tenant selection)
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        if (roleClaim?.Value == "SuperAdmin")
        {
            // Check header first
            if (Request.Headers.TryGetValue("X-Project-Id", out var headerValue) && 
                Guid.TryParse(headerValue.FirstOrDefault(), out var headerProjectId))
            {
                return headerProjectId;
            }
            
            // Fallback to first available project
            var firstProject = await _unitOfWork.Repository<Project>()
                .Query()
                .OrderBy(p => p.CreatedAt)
                .FirstOrDefaultAsync();
            
            return firstProject?.Id;
        }

        // For other users, get from JWT claim
        var projectIdClaim = User.FindFirst("ProjectId");
        if (projectIdClaim != null && Guid.TryParse(projectIdClaim.Value, out var projectId))
        {
            return projectId;
        }

        return null;
    }
}
