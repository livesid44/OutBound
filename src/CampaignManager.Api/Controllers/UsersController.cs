using System.Security.Claims;
using CampaignManager.Api.Services;
using CampaignManager.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampaignManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var projectId = GetCurrentProjectId();
        var result = await _userService.GetUsersAsync(projectId, page, pageSize);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,SubAdmin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create([FromBody] CreateUserRequest request)
    {
        var projectId = GetCurrentProjectId();
        if (projectId == null)
        {
            return BadRequest(ApiResponse<UserDto>.ErrorResponse("Project context required"));
        }

        var result = await _userService.CreateUserAsync(projectId.Value, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,SubAdmin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var result = await _userService.UpdateUserAsync(id, request);
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
        var result = await _userService.DeleteUserAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("bulk")]
    [Authorize(Roles = "SuperAdmin,SubAdmin")]
    public async Task<ActionResult<BulkUserUploadResult>> BulkCreate([FromBody] BulkUserUploadRequest request)
    {
        var projectId = GetCurrentProjectId();
        if (projectId == null)
        {
            return BadRequest("Project context required");
        }

        var result = await _userService.BulkCreateUsersAsync(projectId.Value, request);
        return Ok(result);
    }

    [HttpGet("supervisors")]
    public async Task<ActionResult<List<UserDto>>> GetSupervisors()
    {
        var projectId = GetCurrentProjectId();
        if (projectId == null)
        {
            return Ok(new List<UserDto>());
        }

        var result = await _userService.GetSupervisorsAsync(projectId.Value);
        return Ok(result);
    }

    [HttpGet("subordinates/{supervisorId:guid}")]
    public async Task<ActionResult<List<UserDto>>> GetSubordinates(Guid supervisorId)
    {
        var result = await _userService.GetSubordinatesAsync(supervisorId);
        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _userService.GetByIdAsync(userId.Value);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
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
