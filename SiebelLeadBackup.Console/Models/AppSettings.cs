namespace SiebelLeadBackup.Console.Models;

public class AppSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public SiebelApiSettings SiebelApi { get; set; } = new();
    public ProcessingSettings Processing { get; set; } = new();
}

public class SiebelApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Authorization { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}

public class ProcessingSettings
{
    public int MaxDegreeOfParallelism { get; set; } = 5;
    public int BatchSize { get; set; } = 100;
}
