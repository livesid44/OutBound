using System.Security.Claims;
using CampaignManager.Api.Services;
using CampaignManager.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampaignManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var projectId = GetCurrentProjectId();
        if (projectId == null)
        {
            return BadRequest("Project context required");
        }

        var result = await _dashboardService.GetDashboardSummaryAsync(projectId.Value);
        return Ok(result);
    }

    [HttpGet("supervisor")]
    [Authorize(Roles = "SuperAdmin,SubAdmin,Supervisor")]
    public async Task<ActionResult<SupervisorDashboardDto>> GetSupervisorDashboard()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _dashboardService.GetSupervisorDashboardAsync(userId.Value);
        return Ok(result);
    }

    [HttpGet("agent-performance")]
    [Authorize(Roles = "SuperAdmin,SubAdmin,Supervisor")]
    public async Task<ActionResult<List<AgentPerformanceDto>>> GetAgentPerformance()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _dashboardService.GetAgentPerformanceAsync(userId.Value);
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
}
