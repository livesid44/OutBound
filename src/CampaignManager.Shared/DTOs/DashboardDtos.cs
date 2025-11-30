using CampaignManager.Shared.Models;

namespace CampaignManager.Shared.DTOs;

public class CampaignStatsDto
{
    public Guid CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public int TotalLeads { get; set; }
    public int QueuedCount { get; set; }
    public int InProgressCount { get; set; }
    public int EmailPendingCount { get; set; }
    public int EmailSentCount { get; set; }
    public int EmailFailedCount { get; set; }
    public int CallPendingCount { get; set; }
    public int DialedCount { get; set; }
    public int ConnectedCount { get; set; }
    public int NoAnswerCount { get; set; }
    public int RightPartyContactCount { get; set; }
    public int DisposedCount { get; set; }
    public int FailedCount { get; set; }
}

public class QueueStatusDto
{
    public Guid CampaignId { get; set; }
    public int EmailQueueCount { get; set; }
    public int VoiceQueueCount { get; set; }
    public DateTime? LastProcessedAt { get; set; }
    public DateTime? NextScheduledAt { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class AgentPerformanceDto
{
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public int TotalCallsHandled { get; set; }
    public int SuccessfulDispositions { get; set; }
    public int FailedDispositions { get; set; }
    public double AverageCallDuration { get; set; }
    public DateTime? LastActivityAt { get; set; }
}

public class SupervisorDashboardDto
{
    public List<AgentPerformanceDto> TeamPerformance { get; set; } = new();
    public int OnlineAgentCount { get; set; }
    public int OfflineAgentCount { get; set; }
    public int TotalCallsToday { get; set; }
    public int SuccessfulCallsToday { get; set; }
}

public class DashboardSummaryDto
{
    public int TotalCampaigns { get; set; }
    public int ActiveCampaigns { get; set; }
    public int TotalLeads { get; set; }
    public int ProcessedLeads { get; set; }
    public int PendingLeads { get; set; }
    public List<CampaignStatsDto> CampaignStats { get; set; } = new();
}
