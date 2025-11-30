using CampaignManager.Data.Repositories;
using CampaignManager.Shared.DTOs;
using CampaignManager.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CampaignManager.Api.Services;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<PagedResult<UserDto>> GetUsersAsync(Guid? projectId, int page, int pageSize);
    Task<ApiResponse<UserDto>> CreateUserAsync(Guid projectId, CreateUserRequest request);
    Task<ApiResponse<UserDto>> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<ApiResponse<bool>> DeleteUserAsync(Guid id);
    Task<BulkUserUploadResult> BulkCreateUsersAsync(Guid projectId, BulkUserUploadRequest request);
    Task<List<UserDto>> GetSupervisorsAsync(Guid projectId);
    Task<List<UserDto>> GetSubordinatesAsync(Guid supervisorId);
}

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _unitOfWork.Repository<User>()
            .Query()
            .Include(u => u.Supervisor)
            .FirstOrDefaultAsync(u => u.Id == id);

        return user != null ? MapToDto(user) : null;
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(Guid? projectId, int page, int pageSize)
    {
        var query = _unitOfWork.Repository<User>().Query();

        if (projectId.HasValue)
        {
            query = query.Where(u => u.ProjectId == projectId.Value);
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .Include(u => u.Supervisor)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<UserDto>
        {
            Items = users.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<ApiResponse<UserDto>> CreateUserAsync(Guid projectId, CreateUserRequest request)
    {
        // Check license limits
        var license = await _unitOfWork.Repository<License>()
            .FirstOrDefaultAsync(l => l.ProjectId == projectId);

        if (license != null)
        {
            var currentCount = await _unitOfWork.Repository<User>()
                .CountAsync(u => u.ProjectId == projectId);

            if (currentCount >= license.MaxUsers)
            {
                return ApiResponse<UserDto>.ErrorResponse("Maximum user limit reached for this project");
            }
        }

        var existingUser = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

        if (existingUser != null)
        {
            return ApiResponse<UserDto>.ErrorResponse("Email already exists");
        }

        var user = new User
        {
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role,
            SupervisorId = request.SupervisorId,
            ProjectId = projectId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<UserDto>.SuccessResponse(MapToDto(user));
    }

    public async Task<ApiResponse<UserDto>> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
        if (user == null)
        {
            return ApiResponse<UserDto>.ErrorResponse("User not found");
        }

        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;
        if (request.Role.HasValue) user.Role = request.Role.Value;
        if (request.SupervisorId.HasValue) user.SupervisorId = request.SupervisorId.Value;
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<UserDto>.SuccessResponse(MapToDto(user));
    }

    public async Task<ApiResponse<bool>> DeleteUserAsync(Guid id)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
        if (user == null)
        {
            return ApiResponse<bool>.ErrorResponse("User not found");
        }

        _unitOfWork.Repository<User>().Remove(user);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task<BulkUserUploadResult> BulkCreateUsersAsync(Guid projectId, BulkUserUploadRequest request)
    {
        var result = new BulkUserUploadResult
        {
            TotalCount = request.Users.Count
        };

        // Check license limits
        var license = await _unitOfWork.Repository<License>()
            .FirstOrDefaultAsync(l => l.ProjectId == projectId);

        var currentCount = await _unitOfWork.Repository<User>()
            .CountAsync(u => u.ProjectId == projectId);

        var availableSlots = license?.MaxUsers - currentCount ?? int.MaxValue;

        foreach (var userRequest in request.Users)
        {
            if (result.SuccessCount >= availableSlots)
            {
                result.Errors.Add($"User limit reached. Remaining users not created.");
                result.FailedCount += request.Users.Count - result.SuccessCount;
                break;
            }

            try
            {
                var existingUser = await _unitOfWork.Repository<User>()
                    .FirstOrDefaultAsync(u => u.Email == userRequest.Email.ToLower());

                if (existingUser != null)
                {
                    result.FailedCount++;
                    result.Errors.Add($"Email {userRequest.Email} already exists");
                    continue;
                }

                var user = new User
                {
                    Email = userRequest.Email.ToLower(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRequest.Password),
                    FirstName = userRequest.FirstName,
                    LastName = userRequest.LastName,
                    Role = userRequest.Role,
                    SupervisorId = userRequest.SupervisorId,
                    ProjectId = projectId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<User>().AddAsync(user);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.Errors.Add($"Error creating user {userRequest.Email}: {ex.Message}");
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return result;
    }

    public async Task<List<UserDto>> GetSupervisorsAsync(Guid projectId)
    {
        var supervisors = await _unitOfWork.Repository<User>()
            .Query()
            .Where(u => u.ProjectId == projectId &&
                       (u.Role == UserRole.Supervisor || u.Role == UserRole.SubAdmin))
            .OrderBy(u => u.LastName)
            .ToListAsync();

        return supervisors.Select(MapToDto).ToList();
    }

    public async Task<List<UserDto>> GetSubordinatesAsync(Guid supervisorId)
    {
        var subordinates = await _unitOfWork.Repository<User>()
            .Query()
            .Include(u => u.Supervisor)
            .Where(u => u.SupervisorId == supervisorId)
            .OrderBy(u => u.LastName)
            .ToListAsync();

        return subordinates.Select(MapToDto).ToList();
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            ProjectId = user.ProjectId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            SupervisorId = user.SupervisorId,
            SupervisorName = user.Supervisor != null ? $"{user.Supervisor.FirstName} {user.Supervisor.LastName}" : null,
            IsActive = user.IsActive,
            IsOnline = user.IsOnline,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };
    }
}
