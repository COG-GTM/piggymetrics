using PiggyMetrics.CurrencyExchange.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<IExchangeRateProvider, ExchangeRateProvider>(client =>
{
    var rateApiUrl = Environment.GetEnvironmentVariable("EXCHANGE_RATE_API_URL")
        ?? "https://api.exchangerate.host";
    client.BaseAddress = new Uri(rateApiUrl);
});
builder.Services.AddScoped<ICurrencyConversionService, CurrencyConversionService>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Currency Exchange Service",
        Version = "v1",
        Description = "Real-time currency conversion and exchange rate management for multi-currency financial operations"
    });
});

builder.WebHost.UseUrls("http://0.0.0.0:8087");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Currency Exchange API V1");
});

app.MapControllers();

app.Run();
