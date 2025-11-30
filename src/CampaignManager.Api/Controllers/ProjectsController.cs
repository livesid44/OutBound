using CampaignManager.Api.Services;
using CampaignManager.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampaignManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }
        return Ok(project);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProjectDto>>> GetProjects([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _projectService.GetProjectsAsync(page, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> Create([FromBody] CreateProjectRequest request)
    {
        var result = await _projectService.CreateProjectAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> Update(Guid id, [FromBody] UpdateProjectRequest request)
    {
        var result = await _projectService.UpdateProjectAsync(id, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _projectService.DeleteProjectAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("{id:guid}/license")]
    public async Task<ActionResult<LicenseDto>> GetLicense(Guid id)
    {
        var license = await _projectService.GetLicenseAsync(id);
        if (license == null)
        {
            return NotFound();
        }
        return Ok(license);
    }

    [HttpPost("{id:guid}/license/regenerate")]
    public async Task<ActionResult<string>> RegenerateLicenseKey(
        Guid id,
        [FromQuery] int maxUsers,
        [FromQuery] int maxCampaigns,
        [FromQuery] DateTime expiryDate)
    {
        var licenseKey = await _projectService.GenerateLicenseKeyAsync(id, maxUsers, maxCampaigns, expiryDate);
        return Ok(new { LicenseKey = licenseKey });
    }
}
