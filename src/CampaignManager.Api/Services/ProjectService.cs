using CampaignManager.Data.Repositories;
using CampaignManager.Shared.DTOs;
using CampaignManager.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CampaignManager.Api.Services;

public interface IProjectService
{
    Task<ProjectDto?> GetByIdAsync(Guid id);
    Task<PagedResult<ProjectDto>> GetProjectsAsync(int page, int pageSize);
    Task<ApiResponse<ProjectDto>> CreateProjectAsync(CreateProjectRequest request);
    Task<ApiResponse<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request);
    Task<ApiResponse<bool>> DeleteProjectAsync(Guid id);
    Task<LicenseDto?> GetLicenseAsync(Guid projectId);
    Task<string> GenerateLicenseKeyAsync(Guid projectId, int maxUsers, int maxCampaigns, DateTime expiryDate);
}

public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProjectService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectDto?> GetByIdAsync(Guid id)
    {
        var project = await _unitOfWork.Repository<Project>()
            .Query()
            .Include(p => p.License)
            .Include(p => p.Users)
            .Include(p => p.Campaigns)
            .FirstOrDefaultAsync(p => p.Id == id);

        return project != null ? MapToDto(project) : null;
    }

    public async Task<PagedResult<ProjectDto>> GetProjectsAsync(int page, int pageSize)
    {
        var query = _unitOfWork.Repository<Project>().Query();

        var totalCount = await query.CountAsync();
        var projects = await query
            .Include(p => p.License)
            .Include(p => p.Users)
            .Include(p => p.Campaigns)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ProjectDto>
        {
            Items = projects.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<ApiResponse<ProjectDto>> CreateProjectAsync(CreateProjectRequest request)
    {
        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Project>().AddAsync(project);

        // Create license
        var license = new License
        {
            ProjectId = project.Id,
            LicenseKey = GenerateLicenseKey(),
            MaxUsers = request.MaxUsers,
            MaxCampaigns = request.MaxCampaigns,
            ExpiryDate = request.LicenseExpiryDate,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<License>().AddAsync(license);
        await _unitOfWork.SaveChangesAsync();

        project.License = license;

        return ApiResponse<ProjectDto>.SuccessResponse(MapToDto(project));
    }

    public async Task<ApiResponse<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request)
    {
        var project = await _unitOfWork.Repository<Project>()
            .Query()
            .Include(p => p.License)
            .Include(p => p.Users)
            .Include(p => p.Campaigns)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            return ApiResponse<ProjectDto>.ErrorResponse("Project not found");
        }

        if (request.Name != null) project.Name = request.Name;
        if (request.Description != null) project.Description = request.Description;
        if (request.IsActive.HasValue) project.IsActive = request.IsActive.Value;

        project.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Project>().Update(project);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<ProjectDto>.SuccessResponse(MapToDto(project));
    }

    public async Task<ApiResponse<bool>> DeleteProjectAsync(Guid id)
    {
        var project = await _unitOfWork.Repository<Project>().GetByIdAsync(id);
        if (project == null)
        {
            return ApiResponse<bool>.ErrorResponse("Project not found");
        }

        _unitOfWork.Repository<Project>().Remove(project);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task<LicenseDto?> GetLicenseAsync(Guid projectId)
    {
        var license = await _unitOfWork.Repository<License>()
            .FirstOrDefaultAsync(l => l.ProjectId == projectId);

        return license != null ? MapLicenseToDto(license) : null;
    }

    public async Task<string> GenerateLicenseKeyAsync(Guid projectId, int maxUsers, int maxCampaigns, DateTime expiryDate)
    {
        var existingLicense = await _unitOfWork.Repository<License>()
            .FirstOrDefaultAsync(l => l.ProjectId == projectId);

        if (existingLicense != null)
        {
            existingLicense.LicenseKey = GenerateLicenseKey();
            existingLicense.MaxUsers = maxUsers;
            existingLicense.MaxCampaigns = maxCampaigns;
            existingLicense.ExpiryDate = expiryDate;
            _unitOfWork.Repository<License>().Update(existingLicense);
        }
        else
        {
            var license = new License
            {
                ProjectId = projectId,
                LicenseKey = GenerateLicenseKey(),
                MaxUsers = maxUsers,
                MaxCampaigns = maxCampaigns,
                ExpiryDate = expiryDate,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<License>().AddAsync(license);
        }

        await _unitOfWork.SaveChangesAsync();

        return (await _unitOfWork.Repository<License>()
            .FirstOrDefaultAsync(l => l.ProjectId == projectId))!.LicenseKey;
    }

    private static string GenerateLicenseKey()
    {
        return $"{Guid.NewGuid():N}"[..8].ToUpper() + "-" +
               $"{Guid.NewGuid():N}"[..4].ToUpper() + "-" +
               $"{Guid.NewGuid():N}"[..4].ToUpper() + "-" +
               $"{Guid.NewGuid():N}"[..8].ToUpper();
    }

    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            IsActive = project.IsActive,
            CreatedAt = project.CreatedAt,
            License = project.License != null ? MapLicenseToDto(project.License) : null,
            UserCount = project.Users?.Count ?? 0,
            CampaignCount = project.Campaigns?.Count ?? 0
        };
    }

    private static LicenseDto MapLicenseToDto(License license)
    {
        return new LicenseDto
        {
            Id = license.Id,
            LicenseKey = license.LicenseKey,
            MaxUsers = license.MaxUsers,
            MaxCampaigns = license.MaxCampaigns,
            ExpiryDate = license.ExpiryDate,
            IsActivated = license.IsActivated,
            ActivatedAt = license.ActivatedAt
        };
    }
}
