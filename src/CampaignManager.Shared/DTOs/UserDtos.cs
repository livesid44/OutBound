using CampaignManager.Shared.Models;

namespace CampaignManager.Shared.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public Guid? ProjectId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? SupervisorId { get; set; }
    public string? SupervisorName { get; set; }
    public bool IsActive { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? SupervisorId { get; set; }
}

public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserRole? Role { get; set; }
    public Guid? SupervisorId { get; set; }
    public bool? IsActive { get; set; }
}

public class BulkUserUploadRequest
{
    public List<CreateUserRequest> Users { get; set; } = new();
}

public class BulkUserUploadResult
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
