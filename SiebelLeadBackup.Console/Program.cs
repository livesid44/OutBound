using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SiebelLeadBackup.Console.Models;
using SiebelLeadBackup.Console.Services;

Console.WriteLine("=== Siebel Lead Backup Processing Application ===");
Console.WriteLine($"Starting at: {DateTime.Now}");
Console.WriteLine($"Application Path: {Directory.GetCurrentDirectory()}");
Console.WriteLine();

IConfiguration? configuration = null;
AppSettings? appSettings = null;
ILoggerFactory? loggerFactory = null;

try
{
    Console.WriteLine("Loading configuration from appsettings.json...");
    configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();
    Console.WriteLine("Configuration loaded successfully");

    Console.WriteLine("Parsing application settings...");
    appSettings = new AppSettings
    {
        ConnectionString = configuration["ConnectionString"] ?? throw new Exception("ConnectionString not found in configuration"),
        SiebelApi = configuration.GetSection("SiebelApi").Get<SiebelApiSettings>() ?? throw new Exception("SiebelApi settings not found"),
        Processing = configuration.GetSection("Processing").Get<ProcessingSettings>() ?? throw new Exception("Processing settings not found"),
        Logging = configuration.GetSection("Logging").Get<LoggingSettings>() ?? new LoggingSettings()
    };
    Console.WriteLine("Application settings parsed successfully");

    // Ensure log directory exists
    if (appSettings.Logging.EnableFileLogging)
    {
        Console.WriteLine("Setting up file logging...");
        var logFilePath = appSettings.Logging.LogFilePath.Replace("{Date}", DateTime.Now.ToString("yyyy-MM-dd"));
        var logDirectory = Path.GetDirectoryName(logFilePath);
        if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
            Console.WriteLine($"Created log directory: {logDirectory}");
        }
        Console.WriteLine($"Log file path: {logFilePath}");
    }

    Console.WriteLine("Initializing logging system...");
    loggerFactory = LoggerFactory.Create(builder =>
    {
        builder
            .AddConfiguration(configuration.GetSection("Logging:LogLevel"))
            .AddConsole();
        
        if (appSettings.Logging.EnableFileLogging)
        {
            try
            {
                builder.AddFile(appSettings.Logging.LogFilePath.Replace("{Date}", DateTime.Now.ToString("yyyy-MM-dd")));
            }
            catch (Exception logEx)
            {
                Console.WriteLine($"Warning: Could not initialize file logging: {logEx.Message}");
            }
        }
    });
    Console.WriteLine("Logging system initialized successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"FATAL ERROR during initialization: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    Console.WriteLine();
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    Environment.Exit(1);
    return;
}

if (configuration == null || appSettings == null || loggerFactory == null)
{
    Console.WriteLine("FATAL ERROR: Failed to initialize application components");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    Environment.Exit(1);
    return;
}

try
{
    Console.WriteLine("Creating service instances...");
    var databaseLogger = loggerFactory.CreateLogger<DatabaseService>();
    var siebelLogger = loggerFactory.CreateLogger<SiebelApiService>();
    var processingLogger = loggerFactory.CreateLogger<LeadProcessingService>();

    var databaseService = new DatabaseService(appSettings.ConnectionString, databaseLogger);
    var httpClient = new HttpClient();
    
    // Validate Siebel API URL before creating service
    if (string.IsNullOrWhiteSpace(appSettings.SiebelApi.BaseUrl) || 
        !Uri.TryCreate(appSettings.SiebelApi.BaseUrl, UriKind.Absolute, out _))
    {
        throw new Exception($"Invalid Siebel API BaseUrl: {appSettings.SiebelApi.BaseUrl}");
    }
    
    var siebelApiService = new SiebelApiService(httpClient, appSettings.SiebelApi, siebelLogger);
    var leadProcessingService = new LeadProcessingService(databaseService, siebelApiService, appSettings.Processing, processingLogger);
    
    Console.WriteLine("All services created successfully");
    Console.WriteLine();
    Console.WriteLine($"Max Parallelism: {appSettings.Processing.MaxDegreeOfParallelism}");
    Console.WriteLine($"Database Connection: {(string.IsNullOrEmpty(appSettings.ConnectionString) ? "NOT CONFIGURED" : "Configured")}");
    Console.WriteLine($"Siebel API: {appSettings.SiebelApi.BaseUrl}");
    Console.WriteLine();
    Console.WriteLine("Starting lead processing...");
    Console.WriteLine();

    await leadProcessingService.ProcessLeadsAsync();
    
    Console.WriteLine();
    Console.WriteLine("===========================================");
    Console.WriteLine($"Processing completed at: {DateTime.Now}");
    Console.WriteLine("===========================================");
    Console.WriteLine();
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("===========================================");
    Console.WriteLine("FATAL ERROR OCCURRED");
    Console.WriteLine("===========================================");
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine();
    Console.WriteLine($"Exception Type: {ex.GetType().Name}");
    Console.WriteLine();
    Console.WriteLine("Stack Trace:");
    Console.WriteLine(ex.StackTrace);
    
    if (ex.InnerException != null)
    {
        Console.WriteLine();
        Console.WriteLine("Inner Exception:");
        Console.WriteLine($"  {ex.InnerException.Message}");
        Console.WriteLine($"  {ex.InnerException.StackTrace}");
    }
    
    Console.WriteLine();
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    Environment.Exit(1);
}
