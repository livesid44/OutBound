using CampaignManager.Data.Repositories;
using CampaignManager.Shared.DTOs;
using CampaignManager.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CampaignManager.Api.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid projectId);
    Task<SupervisorDashboardDto> GetSupervisorDashboardAsync(Guid supervisorId);
    Task<List<AgentPerformanceDto>> GetAgentPerformanceAsync(Guid supervisorId);
}

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid projectId)
    {
        var campaigns = await _unitOfWork.Repository<Campaign>()
            .Query()
            .Where(c => c.ProjectId == projectId)
            .Include(c => c.Leads)
            .ToListAsync();

        var summary = new DashboardSummaryDto
        {
            TotalCampaigns = campaigns.Count,
            ActiveCampaigns = campaigns.Count(c => c.Status == CampaignStatus.Active),
            TotalLeads = campaigns.Sum(c => c.Leads?.Count ?? 0),
            ProcessedLeads = campaigns.Sum(c => c.Leads?.Count(l => l.Status == LeadStatus.Disposed) ?? 0),
            PendingLeads = campaigns.Sum(c => c.Leads?.Count(l => l.Status != LeadStatus.Disposed && l.Status != LeadStatus.Failed) ?? 0),
            CampaignStats = campaigns.Select(c => new CampaignStatsDto
            {
                CampaignId = c.Id,
                CampaignName = c.Name,
                TotalLeads = c.Leads?.Count ?? 0,
                QueuedCount = c.Leads?.Count(l => l.Status == LeadStatus.Queued) ?? 0,
                EmailPendingCount = c.Leads?.Count(l => l.Status == LeadStatus.EmailPending) ?? 0,
                EmailSentCount = c.Leads?.Count(l => l.Status == LeadStatus.EmailSent) ?? 0,
                CallPendingCount = c.Leads?.Count(l => l.Status == LeadStatus.CallPending) ?? 0,
                DialedCount = c.Leads?.Count(l => l.Status == LeadStatus.Dialed) ?? 0,
                ConnectedCount = c.Leads?.Count(l => l.Status == LeadStatus.Connected) ?? 0,
                RightPartyContactCount = c.Leads?.Count(l => l.Status == LeadStatus.RightPartyContact) ?? 0,
                DisposedCount = c.Leads?.Count(l => l.Status == LeadStatus.Disposed) ?? 0,
                FailedCount = c.Leads?.Count(l => l.Status == LeadStatus.Failed) ?? 0
            }).ToList()
        };

        return summary;
    }

    public async Task<SupervisorDashboardDto> GetSupervisorDashboardAsync(Guid supervisorId)
    {
        var agents = await _unitOfWork.Repository<User>()
            .Query()
            .Where(u => u.SupervisorId == supervisorId)
            .ToListAsync();

        var agentIds = agents.Select(a => a.Id).ToList();

        var dispositions = await _unitOfWork.Repository<LeadDisposition>()
            .Query()
            .Where(d => d.DisposedById.HasValue && agentIds.Contains(d.DisposedById.Value))
            .Where(d => d.CreatedAt.Date == DateTime.UtcNow.Date)
            .ToListAsync();

        var teamPerformance = agents.Select(a => new AgentPerformanceDto
        {
            AgentId = a.Id,
            AgentName = $"{a.FirstName} {a.LastName}",
            IsOnline = a.IsOnline,
            TotalCallsHandled = dispositions.Count(d => d.DisposedById == a.Id),
            SuccessfulDispositions = dispositions.Count(d => d.DisposedById == a.Id),
            LastActivityAt = a.LastLoginAt
        }).ToList();

        return new SupervisorDashboardDto
        {
            TeamPerformance = teamPerformance,
            OnlineAgentCount = agents.Count(a => a.IsOnline),
            OfflineAgentCount = agents.Count(a => !a.IsOnline),
            TotalCallsToday = dispositions.Count,
            SuccessfulCallsToday = dispositions.Count
        };
    }

    public async Task<List<AgentPerformanceDto>> GetAgentPerformanceAsync(Guid supervisorId)
    {
        var agents = await _unitOfWork.Repository<User>()
            .Query()
            .Where(u => u.SupervisorId == supervisorId)
            .ToListAsync();

        var agentIds = agents.Select(a => a.Id).ToList();

        var dispositions = await _unitOfWork.Repository<LeadDisposition>()
            .Query()
            .Where(d => d.DisposedById.HasValue && agentIds.Contains(d.DisposedById.Value))
            .ToListAsync();

        return agents.Select(a => new AgentPerformanceDto
        {
            AgentId = a.Id,
            AgentName = $"{a.FirstName} {a.LastName}",
            IsOnline = a.IsOnline,
            TotalCallsHandled = dispositions.Count(d => d.DisposedById == a.Id),
            SuccessfulDispositions = dispositions.Count(d => d.DisposedById == a.Id),
            LastActivityAt = a.LastLoginAt
        }).ToList();
    }
}
