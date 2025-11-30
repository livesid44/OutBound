using System.Security.Claims;
using CampaignManager.Api.Services;
using CampaignManager.Shared.DTOs;
using CampaignManager.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampaignManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeadsController : ControllerBase
{
    private readonly ILeadService _leadService;

    public LeadsController(ILeadService leadService)
    {
        _leadService = leadService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LeadDto>> GetById(Guid id)
    {
        var lead = await _leadService.GetByIdAsync(id);
        if (lead == null)
        {
            return NotFound();
        }
        return Ok(lead);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<LeadDto>>> UpdateStatus(Guid id, [FromQuery] LeadStatus status)
    {
        var result = await _leadService.UpdateLeadStatusAsync(id, status);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("{id:guid}/dispose")]
    public async Task<ActionResult<ApiResponse<LeadDto>>> Dispose(Guid id, [FromBody] CreateDispositionRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _leadService.DisposeLeadAsync(id, userId.Value, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
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
}
