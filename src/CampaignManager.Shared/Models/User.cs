using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampaignManager.Shared.Models;

/// <summary>
/// User roles in the system
/// </summary>
public enum UserRole
{
    SuperAdmin = 0,
    SubAdmin = 1,
    Supervisor = 2,
    Agent = 3
}

/// <summary>
/// Represents a user in the system
/// </summary>
public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? ProjectId { get; set; }

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Agent;

    public Guid? SupervisorId { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsOnline { get; set; } = false;

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }

    [ForeignKey(nameof(SupervisorId))]
    public virtual User? Supervisor { get; set; }

    public virtual ICollection<User> Subordinates { get; set; } = new List<User>();
}
