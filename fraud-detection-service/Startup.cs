using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PiggyMetrics.FraudDetection.Repository;
using PiggyMetrics.FraudDetection.Services;
using Swashbuckle.AspNetCore.Swagger;

namespace PiggyMetrics.FraudDetection
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
            var mongoHost = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "fraud-mongodb";
            var mongoPassword = Environment.GetEnvironmentVariable("MONGODB_PASSWORD") ?? "piggymetrics";
            var connectionString = $"mongodb://user:{mongoPassword}@{mongoHost}:27017/fraud";
            services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
            services.AddSingleton<IMongoDatabase>(sp =>
                sp.GetRequiredService<IMongoClient>().GetDatabase("fraud"));

            // Services
            services.AddSingleton<IFraudRuleRepository, FraudRuleRepository>();
            services.AddSingleton<IFraudDetectionService, FraudDetectionServiceImpl>();
            services.AddSingleton<IAccountServiceClient, AccountServiceClient>();

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Fraud Detection Service",
                    Version = "v1",
                    Description = "Analyzes transactions for suspicious patterns and potential fraud"
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fraud Detection API V1");
            });

            app.UseMvc();
        }
    }
}
