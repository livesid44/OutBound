using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SiebelLeadBackup.Console.Models;
using SiebelLeadBackup.Console.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var appSettings = new AppSettings
{
    ConnectionString = configuration["ConnectionString"] ?? throw new Exception("ConnectionString not found in configuration"),
    SiebelApi = configuration.GetSection("SiebelApi").Get<SiebelApiSettings>() ?? throw new Exception("SiebelApi settings not found"),
    Processing = configuration.GetSection("Processing").Get<ProcessingSettings>() ?? throw new Exception("Processing settings not found"),
    Logging = configuration.GetSection("Logging").Get<LoggingSettings>() ?? new LoggingSettings()
};

// Ensure log directory exists
if (appSettings.Logging.EnableFileLogging)
{
    var logFilePath = appSettings.Logging.LogFilePath.Replace("{Date}", DateTime.Now.ToString("yyyy-MM-dd"));
    var logDirectory = Path.GetDirectoryName(logFilePath);
    if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
    {
        Directory.CreateDirectory(logDirectory);
    }
}

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConfiguration(configuration.GetSection("Logging:LogLevel"))
        .AddConsole();
    
    if (appSettings.Logging.EnableFileLogging)
    {
        builder.AddFile(appSettings.Logging.LogFilePath.Replace("{Date}", DateTime.Now.ToString("yyyy-MM-dd")));
    }
});

var databaseLogger = loggerFactory.CreateLogger<DatabaseService>();
var siebelLogger = loggerFactory.CreateLogger<SiebelApiService>();
var processingLogger = loggerFactory.CreateLogger<LeadProcessingService>();

var databaseService = new DatabaseService(appSettings.ConnectionString, databaseLogger);
var httpClient = new HttpClient();
var siebelApiService = new SiebelApiService(httpClient, appSettings.SiebelApi, siebelLogger);
var leadProcessingService = new LeadProcessingService(databaseService, siebelApiService, appSettings.Processing, processingLogger);

Console.WriteLine("=== Siebel Lead Backup Processing Application ===");
Console.WriteLine($"Starting at: {DateTime.Now}");
Console.WriteLine($"Max Parallelism: {appSettings.Processing.MaxDegreeOfParallelism}");
Console.WriteLine();

try
{
    await leadProcessingService.ProcessLeadsAsync();
    Console.WriteLine();
    Console.WriteLine($"Completed at: {DateTime.Now}");
    Console.WriteLine("Press any key to exit...");
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    Environment.Exit(1);
}

Console.ReadKey();
