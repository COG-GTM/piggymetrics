using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PiggyMetrics.Compliance.Repository;
using PiggyMetrics.Compliance.Services;
using Swashbuckle.AspNetCore.Swagger;

namespace PiggyMetrics.Compliance
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // MongoDB
            var mongoHost = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "compliance-mongodb";
            var mongoPassword = Environment.GetEnvironmentVariable("MONGODB_PASSWORD") ?? "piggymetrics";
            var connectionString = $"mongodb://user:{mongoPassword}@{mongoHost}:27017/compliance";
            services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
            services.AddSingleton<IMongoDatabase>(sp =>
                sp.GetRequiredService<IMongoClient>().GetDatabase("compliance"));

            // Services
            services.AddSingleton<IAuditLogRepository, AuditLogRepository>();
            services.AddSingleton<IComplianceRuleRepository, ComplianceRuleRepository>();
            services.AddSingleton<IComplianceService, ComplianceServiceImpl>();

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Compliance Service",
                    Version = "v1",
                    Description = "Regulatory compliance checking, audit logging, and KYC/AML monitoring"
                });
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Compliance API V1");
            });

            app.UseMvc();
        }
    }
}
