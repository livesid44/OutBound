using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CampaignManager.Data.Repositories;
using CampaignManager.Shared.DTOs;
using CampaignManager.Shared.Models;
using Microsoft.IdentityModel.Tokens;

namespace CampaignManager.Api.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<bool>> ActivateLicenseAsync(Guid userId, string licenseKey);
    string GenerateJwtToken(User user);
}

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new LoginResponse { Success = false, ErrorMessage = "Invalid email or password" };
        }

        if (!user.IsActive)
        {
            return new LoginResponse { Success = false, ErrorMessage = "User account is deactivated" };
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.IsOnline = true;
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryInMinutes"] ?? "60");

        return new LoginResponse
        {
            Success = true,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
            User = MapToUserDto(user)
        };
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

        if (existingUser != null)
        {
            return new LoginResponse { Success = false, ErrorMessage = "Email already registered" };
        }

        Guid? projectId = null;

        // If license key provided, find the project and validate
        if (!string.IsNullOrEmpty(request.LicenseKey))
        {
            var license = await _unitOfWork.Repository<License>()
                .FirstOrDefaultAsync(l => l.LicenseKey == request.LicenseKey);

            if (license == null)
            {
                return new LoginResponse { Success = false, ErrorMessage = "Invalid license key" };
            }

            if (license.ExpiryDate < DateTime.UtcNow)
            {
                return new LoginResponse { Success = false, ErrorMessage = "License has expired" };
            }

            // Check user count
            var currentUserCount = await _unitOfWork.Repository<User>()
                .CountAsync(u => u.ProjectId == license.ProjectId);

            if (currentUserCount >= license.MaxUsers)
            {
                return new LoginResponse { Success = false, ErrorMessage = "Maximum user limit reached for this license" };
            }

            projectId = license.ProjectId;

            if (!license.IsActivated)
            {
                license.IsActivated = true;
                license.ActivatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<License>().Update(license);
            }
        }

        var user = new User
        {
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = projectId.HasValue ? UserRole.SubAdmin : UserRole.Agent,
            ProjectId = projectId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryInMinutes"] ?? "60");

        return new LoginResponse
        {
            Success = true,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
            User = MapToUserDto(user)
        };
    }

    public async Task<ApiResponse<bool>> ActivateLicenseAsync(Guid userId, string licenseKey)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<bool>.ErrorResponse("User not found");
        }

        var license = await _unitOfWork.Repository<License>()
            .FirstOrDefaultAsync(l => l.LicenseKey == licenseKey);

        if (license == null)
        {
            return ApiResponse<bool>.ErrorResponse("Invalid license key");
        }

        if (license.ExpiryDate < DateTime.UtcNow)
        {
            return ApiResponse<bool>.ErrorResponse("License has expired");
        }

        var currentUserCount = await _unitOfWork.Repository<User>()
            .CountAsync(u => u.ProjectId == license.ProjectId);

        if (currentUserCount >= license.MaxUsers)
        {
            return ApiResponse<bool>.ErrorResponse("Maximum user limit reached for this license");
        }

        user.ProjectId = license.ProjectId;
        _unitOfWork.Repository<User>().Update(user);

        if (!license.IsActivated)
        {
            license.IsActivated = true;
            license.ActivatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<License>().Update(license);
        }

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "License activated successfully");
    }

    public string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("ProjectId", user.ProjectId?.ToString() ?? "")
        };

        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryInMinutes"] ?? "60");
        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserDto MapToUserDto(User user)
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
            IsActive = user.IsActive,
            IsOnline = user.IsOnline,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };
    }
}
