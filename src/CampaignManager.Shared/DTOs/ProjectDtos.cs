namespace CampaignManager.Shared.DTOs;

public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public LicenseDto? License { get; set; }
    public int UserCount { get; set; }
    public int CampaignCount { get; set; }
}

public class CreateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaxUsers { get; set; }
    public int MaxCampaigns { get; set; }
    public DateTime LicenseExpiryDate { get; set; }
}

public class UpdateProjectRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class LicenseDto
{
    public Guid Id { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public int MaxUsers { get; set; }
    public int MaxCampaigns { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsActivated { get; set; }
    public DateTime? ActivatedAt { get; set; }
}
