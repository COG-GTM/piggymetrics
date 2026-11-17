using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using PiggyMetrics.FraudDetection.Repository;
using PiggyMetrics.FraudDetection.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:8085");

// MongoDB
var mongoHost = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "fraud-mongodb";
var mongoPassword = Environment.GetEnvironmentVariable("MONGODB_PASSWORD") ?? "piggymetrics";
var connectionString = $"mongodb://user:{mongoPassword}@{mongoHost}:27017/fraud";
builder.Services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("fraud"));

// Services
builder.Services.AddSingleton<IFraudRuleRepository, FraudRuleRepository>();
builder.Services.AddSingleton<IFraudDetectionService, FraudDetectionServiceImpl>();

// HttpClientFactory for AccountServiceClient (replaces RestSharp)
var accountServiceUrl = Environment.GetEnvironmentVariable("ACCOUNT_SERVICE_URL")
    ?? "http://account-service:6000";
builder.Services.AddHttpClient<IAccountServiceClient, AccountServiceClient>(client =>
{
    client.BaseAddress = new Uri(accountServiceUrl);
});

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fraud Detection Service",
        Version = "v1",
        Description = "Analyzes transactions for suspicious patterns and potential fraud"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fraud Detection API V1");
});

app.MapControllers();

app.Run();
