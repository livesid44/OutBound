using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CampaignManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dispositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsQualified = table.Column<bool>(type: "bit", nullable: false),
                    RequiresCallback = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dispositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dispositions_Dispositions_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Dispositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dispositions_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Licenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LicenseKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaxUsers = table.Column<int>(type: "int", nullable: false),
                    MaxCampaigns = table.Column<int>(type: "int", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActivated = table.Column<bool>(type: "bit", nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Licenses_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    SupervisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Users_Users_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Channels = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsSchedulerEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LastProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Campaigns_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Campaigns_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Users_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampaignApiConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SendGridApiKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SendGridFromEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SendGridFromName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SendGridTemplateId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmailTemplateHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailSubject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WebExCurl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    WebExApiEndpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WebExAuthToken = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ParameterMappings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokenGenerationConfig = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoiceBlastApiEndpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VoiceBlastApiKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    VoiceBlastCampaignId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VoiceBlastMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoiceBlastParameterMappings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhatsAppApiEndpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WhatsAppApiKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    WhatsAppAccountSid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WhatsAppAuthToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WhatsAppFromNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WhatsAppTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhatsAppTemplateName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SmsApiEndpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SmsApiKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SmsAccountSid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SmsAuthToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SmsFromNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SmsTemplate = table.Column<string>(type: "nvarchar(1600)", maxLength: 1600, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignApiConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignApiConfigs_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FieldType = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignFields_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignLeads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EmailStatus = table.Column<int>(type: "int", nullable: false),
                    EmailAttempts = table.Column<int>(type: "int", nullable: false),
                    LastEmailAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CallStatus = table.Column<int>(type: "int", nullable: false),
                    CallAttempts = table.Column<int>(type: "int", nullable: false),
                    LastCallAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CallConnectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextScheduledAction = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedAgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignLeads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignLeads_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignLeads_Users_AssignedAgentId",
                        column: x => x.AssignedAgentId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CampaignStrategies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaxEmailAttempts = table.Column<int>(type: "int", nullable: false),
                    MaxCallAttempts = table.Column<int>(type: "int", nullable: false),
                    MaxVoiceBlastAttempts = table.Column<int>(type: "int", nullable: false),
                    MaxWhatsAppAttempts = table.Column<int>(type: "int", nullable: false),
                    MaxSmsAttempts = table.Column<int>(type: "int", nullable: false),
                    EmailRetryIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    CallRetryIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    VoiceBlastRetryIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    WhatsAppRetryIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    SmsRetryIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    CallStartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    CallEndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    WorkingDays = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StrategyRulesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignStrategies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignStrategies_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeadDispositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DispositionCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubDispositionCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CallbackDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DisposedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadDispositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadDispositions_CampaignLeads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "CampaignLeads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeadDispositions_Users_DisposedById",
                        column: x => x.DisposedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "UpdatedAt" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000010"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "A sample project for demonstration purposes", true, "Demo Project", null });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "IsOnline", "LastLoginAt", "LastName", "PasswordHash", "ProjectId", "Role", "SupervisorId", "UpdatedAt" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@campaignmanager.local", "Super", true, false, null, "Admin", "$2a$11$WCDqnEksHWQsG7Za1KfMue9sOUHGRPWPV3GKL93AYQcJzIn8oU7ui", null, 0, null, null });

            migrationBuilder.InsertData(
                table: "Dispositions",
                columns: new[] { "Id", "Code", "CreatedAt", "DisplayOrder", "IsActive", "IsQualified", "Name", "ParentId", "ProjectId", "RequiresCallback" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000020"), "QUALIFIED", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, true, "Qualified Lead", null, new Guid("00000000-0000-0000-0000-000000000010"), false },
                    { new Guid("00000000-0000-0000-0000-000000000021"), "NOT_INTERESTED", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, true, false, "Not Interested", null, new Guid("00000000-0000-0000-0000-000000000010"), false },
                    { new Guid("00000000-0000-0000-0000-000000000022"), "CALLBACK", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, true, false, "Callback Required", null, new Guid("00000000-0000-0000-0000-000000000010"), true },
                    { new Guid("00000000-0000-0000-0000-000000000023"), "NO_ANSWER", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, true, false, "No Answer", null, new Guid("00000000-0000-0000-0000-000000000010"), true }
                });

            migrationBuilder.InsertData(
                table: "Licenses",
                columns: new[] { "Id", "ActivatedAt", "CreatedAt", "ExpiryDate", "IsActivated", "LicenseKey", "MaxCampaigns", "MaxUsers", "ProjectId" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000011"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), true, "DEMO-1234-5678-ABCD", 10, 50, new Guid("00000000-0000-0000-0000-000000000010") });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "IsOnline", "LastLoginAt", "LastName", "PasswordHash", "ProjectId", "Role", "SupervisorId", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "subadmin@demo.local", "Sub", true, false, null, "Admin", "$2a$11$wbeagR.onmQYakqSy5tEqO4z6U.E4JCFqndvYL.6OG5hNj6xsKcfi", new Guid("00000000-0000-0000-0000-000000000010"), 1, null, null },
                    { new Guid("00000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "supervisor@demo.local", "Demo", true, false, null, "Supervisor", "$2a$11$pDgWTkRfzZfrKuxJila8u.X6fzxr1K.PFbdkgQrEl5vGHqovzlr1e", new Guid("00000000-0000-0000-0000-000000000010"), 2, null, null }
                });

            migrationBuilder.InsertData(
                table: "Campaigns",
                columns: new[] { "Id", "Channels", "CreatedAt", "CreatedById", "Description", "IsSchedulerEnabled", "LastProcessedAt", "Name", "NextScheduledAt", "ProjectId", "Status", "UpdatedAt" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000030"), 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000002"), "A sample email campaign for demonstration", false, null, "Demo Email Campaign", null, new Guid("00000000-0000-0000-0000-000000000010"), 0, null });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "IsOnline", "LastLoginAt", "LastName", "PasswordHash", "ProjectId", "Role", "SupervisorId", "UpdatedAt" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "agent@demo.local", "Demo", true, false, null, "Agent", "$2a$11$CTCCNGRvH2t2mMI7eW0KCOm0w7RUq3mm/6WTulJ22y0NBz38mlez6", new Guid("00000000-0000-0000-0000-000000000010"), 3, new Guid("00000000-0000-0000-0000-000000000003"), null });

            migrationBuilder.InsertData(
                table: "CampaignApiConfigs",
                columns: new[] { "Id", "CampaignId", "CreatedAt", "EmailSubject", "EmailTemplateHtml", "ParameterMappings", "SendGridApiKey", "SendGridFromEmail", "SendGridFromName", "SendGridTemplateId", "SmsAccountSid", "SmsApiEndpoint", "SmsApiKey", "SmsAuthToken", "SmsFromNumber", "SmsTemplate", "TokenGenerationConfig", "UpdatedAt", "VoiceBlastApiEndpoint", "VoiceBlastApiKey", "VoiceBlastCampaignId", "VoiceBlastMessage", "VoiceBlastParameterMappings", "WebExApiEndpoint", "WebExAuthToken", "WebExCurl", "WhatsAppAccountSid", "WhatsAppApiEndpoint", "WhatsAppApiKey", "WhatsAppAuthToken", "WhatsAppFromNumber", "WhatsAppTemplate", "WhatsAppTemplateName" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000050"), new Guid("00000000-0000-0000-0000-000000000030"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, "noreply@demo.local", "Demo Campaign", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.InsertData(
                table: "CampaignFields",
                columns: new[] { "Id", "CampaignId", "CreatedAt", "DisplayName", "DisplayOrder", "FieldName", "FieldType", "IsRequired" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000040"), new Guid("00000000-0000-0000-0000-000000000030"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "First Name", 1, "firstName", 0, true },
                    { new Guid("00000000-0000-0000-0000-000000000041"), new Guid("00000000-0000-0000-0000-000000000030"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Last Name", 2, "lastName", 0, true },
                    { new Guid("00000000-0000-0000-0000-000000000042"), new Guid("00000000-0000-0000-0000-000000000030"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Email Address", 3, "email", 4, true },
                    { new Guid("00000000-0000-0000-0000-000000000043"), new Guid("00000000-0000-0000-0000-000000000030"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Phone Number", 4, "phone", 5, false }
                });

            migrationBuilder.InsertData(
                table: "CampaignStrategies",
                columns: new[] { "Id", "CallEndTime", "CallRetryIntervalMinutes", "CallStartTime", "CampaignId", "CreatedAt", "EmailRetryIntervalMinutes", "MaxCallAttempts", "MaxEmailAttempts", "MaxSmsAttempts", "MaxVoiceBlastAttempts", "MaxWhatsAppAttempts", "SmsRetryIntervalMinutes", "StrategyRulesJson", "UpdatedAt", "VoiceBlastRetryIntervalMinutes", "WhatsAppRetryIntervalMinutes", "WorkingDays" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000051"), null, 60, null, new Guid("00000000-0000-0000-0000-000000000030"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1440, 5, 3, 3, 3, 3, 60, null, null, 30, 60, "Mon,Tue,Wed,Thu,Fri" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignApiConfigs_CampaignId",
                table: "CampaignApiConfigs",
                column: "CampaignId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignFields_CampaignId_FieldName",
                table: "CampaignFields",
                columns: new[] { "CampaignId", "FieldName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignLeads_AssignedAgentId",
                table: "CampaignLeads",
                column: "AssignedAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignLeads_CampaignId",
                table: "CampaignLeads",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignLeads_NextScheduledAction",
                table: "CampaignLeads",
                column: "NextScheduledAction");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignLeads_Status",
                table: "CampaignLeads",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_CreatedById",
                table: "Campaigns",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_ProjectId",
                table: "Campaigns",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Status",
                table: "Campaigns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignStrategies_CampaignId",
                table: "CampaignStrategies",
                column: "CampaignId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ReceiverId",
                table: "ChatMessages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId_ReceiverId",
                table: "ChatMessages",
                columns: new[] { "SenderId", "ReceiverId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SentAt",
                table: "ChatMessages",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_Dispositions_ParentId",
                table: "Dispositions",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Dispositions_ProjectId_Code",
                table: "Dispositions",
                columns: new[] { "ProjectId", "Code" },
                unique: true,
                filter: "[ProjectId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LeadDispositions_DisposedById",
                table: "LeadDispositions",
                column: "DisposedById");

            migrationBuilder.CreateIndex(
                name: "IX_LeadDispositions_LeadId",
                table: "LeadDispositions",
                column: "LeadId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_LicenseKey",
                table: "Licenses",
                column: "LicenseKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_ProjectId",
                table: "Licenses",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Name",
                table: "Projects",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ProjectId",
                table: "Users",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SupervisorId",
                table: "Users",
                column: "SupervisorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignApiConfigs");

            migrationBuilder.DropTable(
                name: "CampaignFields");

            migrationBuilder.DropTable(
                name: "CampaignStrategies");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "Dispositions");

            migrationBuilder.DropTable(
                name: "LeadDispositions");

            migrationBuilder.DropTable(
                name: "Licenses");

            migrationBuilder.DropTable(
                name: "CampaignLeads");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
