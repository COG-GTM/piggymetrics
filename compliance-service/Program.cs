using MongoDB.Driver;
using NLog;
using NLog.Web;
using PiggyMetrics.Compliance.Repository;
using PiggyMetrics.Compliance.Services;
using Microsoft.OpenApi.Models;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    logger.Debug("Starting Compliance Service");

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // MongoDB
    var mongoHost = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "compliance-mongodb";
    var mongoPassword = Environment.GetEnvironmentVariable("MONGODB_PASSWORD") ?? "piggymetrics";
    var connectionString = $"mongodb://user:{mongoPassword}@{mongoHost}:27017/compliance";
    builder.Services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
    builder.Services.AddSingleton<IMongoDatabase>(sp =>
        sp.GetRequiredService<IMongoClient>().GetDatabase("compliance"));

    // Services
    builder.Services.AddSingleton<IAuditLogRepository, AuditLogRepository>();
    builder.Services.AddSingleton<IComplianceRuleRepository, ComplianceRuleRepository>();
    builder.Services.AddSingleton<IComplianceService, ComplianceServiceImpl>();

    // Controllers
    builder.Services.AddControllers();

    // Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Compliance Service",
            Version = "v1",
            Description = "Regulatory compliance checking, audit logging, and KYC/AML monitoring"
        });
    });

    builder.WebHost.UseUrls("http://0.0.0.0:8086");

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Compliance API V1");
    });

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Application stopped due to exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}
