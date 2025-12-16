namespace SiebelLeadBackup.Console.Models;

public class AppSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public SiebelApiSettings SiebelApi { get; set; } = new();
    public ProcessingSettings Processing { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
}

public class SiebelApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Authorization { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}

public class ProcessingSettings
{
    public int MaxDegreeOfParallelism { get; set; } = 20;
    public int BatchSize { get; set; } = 100;
}

public class LoggingSettings
{
    public string LogFilePath { get; set; } = "Logs/SiebelLeadBackup_{Date}.txt";
    public bool EnableFileLogging { get; set; } = true;
}
