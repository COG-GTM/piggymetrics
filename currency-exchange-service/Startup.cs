using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PiggyMetrics.CurrencyExchange.Services;
using Swashbuckle.AspNetCore.Swagger;

namespace PiggyMetrics.CurrencyExchange
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

            // Memory cache for exchange rates
            services.AddMemoryCache();

            // Services
            services.AddSingleton<IExchangeRateProvider, ExchangeRateProvider>();
            services.AddSingleton<ICurrencyConversionService, CurrencyConversionService>();

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Currency Exchange Service",
                    Version = "v1",
                    Description = "Real-time currency conversion and exchange rate management for multi-currency financial operations"
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Currency Exchange API V1");
            });

            app.UseMvc();
        }
    }
}
