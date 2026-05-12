using PiggyMetrics.CurrencyExchange.Services;

var builder = WebApplication.CreateBuilder(args);

// Memory cache for exchange rates
builder.Services.AddMemoryCache();

// HttpClient for exchange rate API with base address from environment
var rateApiUrl = Environment.GetEnvironmentVariable("EXCHANGE_RATE_API_URL")
    ?? "https://api.exchangerate.host";
builder.Services.AddHttpClient<IExchangeRateProvider, ExchangeRateProvider>(client =>
{
    client.BaseAddress = new Uri(rateApiUrl);
});
builder.Services.AddSingleton<ICurrencyConversionService, CurrencyConversionService>();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
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
