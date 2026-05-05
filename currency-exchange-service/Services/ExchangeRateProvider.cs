using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using PiggyMetrics.CurrencyExchange.Models;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace PiggyMetrics.CurrencyExchange.Services
{
    public interface IExchangeRateProvider
    {
        Task<ExchangeRateTable> GetLatestRates(string baseCurrency);
        Task<ExchangeRate> GetRate(string baseCurrency, string targetCurrency);
        List<SupportedCurrency> GetSupportedCurrencies();
    }

    public class ExchangeRateProvider : IExchangeRateProvider
    {
        private readonly IMemoryCache _cache;
        private readonly RestClient _client;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

        private static readonly Dictionary<string, decimal> _fallbackRates = new Dictionary<string, decimal>
        {
            { "EUR", 1.0m },
            { "USD", 1.08m },
            { "GBP", 0.86m },
            { "JPY", 164.5m },
            { "CHF", 0.97m },
            { "CAD", 1.47m },
            { "AUD", 1.66m },
            { "CNY", 7.82m },
            { "BRL", 5.45m },
            { "MXN", 18.72m },
            { "ARS", 925.50m },
            { "COP", 4250.00m },
            { "PEN", 3.92m },
            { "CLP", 950.00m }
        };

        private static readonly List<SupportedCurrency> _supportedCurrencies = new List<SupportedCurrency>
        {
            new SupportedCurrency { Code = "EUR", Name = "Euro", Symbol = "\u20AC", DecimalPlaces = 2 },
            new SupportedCurrency { Code = "USD", Name = "US Dollar", Symbol = "$", DecimalPlaces = 2 },
            new SupportedCurrency { Code = "GBP", Name = "British Pound", Symbol = "\u00A3", DecimalPlaces = 2 },
            new SupportedCurrency { Code = "JPY", Name = "Japanese Yen", Symbol = "\u00A5", DecimalPlaces = 0 },
            new SupportedCurrency { Code = "CHF", Name = "Swiss Franc", Symbol = "CHF", DecimalPlaces = 2 },
            new SupportedCurrency { Code = "CAD", Name = "Canadian Dollar", Symbol = "CA$", DecimalPlaces = 2 },
            new SupportedCurrency { Code = "AUD", Name = "Australian Dollar", Symbol = "A$", DecimalPlaces = 2 },
            new SupportedCurrency { Code = "CNY", Name = "Chinese Yuan", Symbol = "\u00A5", DecimalPlaces = 2 },
            new SupportedCurrency { Code = "BRL", Name = "Brazilian Real", Symbol = "R$", DecimalPlaces = 2 },
            new SupportedCurrency { Code = "MXN", Name = "Mexican Peso", Symbol = "MX$", DecimalPlaces = 2 },
            new SupportedCurrency { Code = "ARS", Name = "Argentine Peso", Symbol = "AR$", DecimalPlaces = 2 },
            new SupportedCurrency { Code = "COP", Name = "Colombian Peso", Symbol = "CO$", DecimalPlaces = 0 },
            new SupportedCurrency { Code = "PEN", Name = "Peruvian Sol", Symbol = "S/", DecimalPlaces = 2 },
            new SupportedCurrency { Code = "CLP", Name = "Chilean Peso", Symbol = "CL$", DecimalPlaces = 0 }
        };

        public ExchangeRateProvider(IMemoryCache cache)
        {
            _cache = cache;
            var rateApiUrl = Environment.GetEnvironmentVariable("EXCHANGE_RATE_API_URL")
                ?? "https://api.exchangerate.host";
            _client = new RestClient(rateApiUrl);
        }

        public async Task<ExchangeRateTable> GetLatestRates(string baseCurrency)
        {
            var cacheKey = $"rates_{baseCurrency}";

            if (_cache.TryGetValue(cacheKey, out ExchangeRateTable cachedRates))
            {
                return cachedRates;
            }

            try
            {
                var request = new RestRequest($"/latest?base={baseCurrency}", Method.GET);
                var response = await _client.ExecuteTaskAsync(request);

                if (response.IsSuccessful)
                {
                    var json = JObject.Parse(response.Content);
                    var rates = new Dictionary<string, decimal>();

                    foreach (var rate in json["rates"].Children<JProperty>())
                    {
                        if (_fallbackRates.ContainsKey(rate.Name))
                        {
                            rates[rate.Name] = rate.Value.ToObject<decimal>();
                        }
                    }

                    var table = new ExchangeRateTable
                    {
                        BaseCurrency = baseCurrency,
                        Timestamp = DateTime.UtcNow,
                        Rates = rates
                    };

                    _cache.Set(cacheKey, table, _cacheDuration);
                    return table;
                }
            }
            catch (Exception)
            {
                // Fall through to fallback rates
            }

            return GetFallbackRates(baseCurrency);
        }

        public async Task<ExchangeRate> GetRate(string baseCurrency, string targetCurrency)
        {
            var table = await GetLatestRates(baseCurrency);

            if (table.Rates.ContainsKey(targetCurrency))
            {
                return new ExchangeRate
                {
                    BaseCurrency = baseCurrency,
                    TargetCurrency = targetCurrency,
                    Rate = table.Rates[targetCurrency],
                    Timestamp = table.Timestamp,
                    Source = "exchange-rate-api"
                };
            }

            throw new KeyNotFoundException($"Exchange rate not found for {baseCurrency}/{targetCurrency}");
        }

        public List<SupportedCurrency> GetSupportedCurrencies()
        {
            return _supportedCurrencies;
        }

        private ExchangeRateTable GetFallbackRates(string baseCurrency)
        {
            var baseRate = _fallbackRates.ContainsKey(baseCurrency)
                ? _fallbackRates[baseCurrency]
                : 1.0m;

            var rates = new Dictionary<string, decimal>();
            foreach (var kvp in _fallbackRates)
            {
                rates[kvp.Key] = kvp.Value / baseRate;
            }

            return new ExchangeRateTable
            {
                BaseCurrency = baseCurrency,
                Timestamp = DateTime.UtcNow,
                Rates = rates
            };
        }
    }
}
