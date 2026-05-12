using PiggyMetrics.CurrencyExchange.Models;

namespace PiggyMetrics.CurrencyExchange.Services
{
    public interface ICurrencyConversionService
    {
        Task<ConversionResult> Convert(ConversionRequest request);
        Task<decimal> GetConversionFee(string fromCurrency, string toCurrency, decimal amount);
    }

    public class CurrencyConversionService : ICurrencyConversionService
    {
        private readonly IExchangeRateProvider _rateProvider;

        private static readonly decimal BASE_FEE_PERCENTAGE = 0.005m; // 0.5%
        private static readonly decimal MINIMUM_FEE = 0.50m;
        private static readonly decimal LATAM_SURCHARGE = 0.002m; // Additional 0.2% for LATAM currencies

        private static readonly string[] LATAM_CURRENCIES = { "BRL", "MXN", "ARS", "COP", "PEN", "CLP" };

        public CurrencyConversionService(IExchangeRateProvider rateProvider)
        {
            _rateProvider = rateProvider;
        }

        public async Task<ConversionResult> Convert(ConversionRequest request)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be positive");

            if (string.IsNullOrEmpty(request.FromCurrency) || string.IsNullOrEmpty(request.ToCurrency))
                throw new ArgumentException("Currency codes cannot be empty");

            var rate = await _rateProvider.GetRate(request.FromCurrency, request.ToCurrency);
            var fee = await GetConversionFee(request.FromCurrency, request.ToCurrency, request.Amount);

            if (fee >= request.Amount)
                throw new ArgumentException($"Amount {request.Amount} is too small to cover the minimum conversion fee of {fee}");

            var amountAfterFee = request.Amount - fee;
            var convertedAmount = amountAfterFee * rate.Rate;

            return new ConversionResult
            {
                FromCurrency = request.FromCurrency,
                ToCurrency = request.ToCurrency,
                OriginalAmount = request.Amount,
                ConvertedAmount = Math.Round(convertedAmount, GetDecimalPlaces(request.ToCurrency)),
                ExchangeRate = rate.Rate,
                Timestamp = DateTime.UtcNow,
                Fee = fee,
                TotalCost = request.Amount
            };
        }

        public Task<decimal> GetConversionFee(string fromCurrency, string toCurrency, decimal amount)
        {
            var feePercentage = BASE_FEE_PERCENTAGE;

            if (IsLatamCurrency(fromCurrency) || IsLatamCurrency(toCurrency))
            {
                feePercentage += LATAM_SURCHARGE;
            }

            var fee = amount * feePercentage;
            fee = Math.Max(fee, MINIMUM_FEE);

            return Task.FromResult(Math.Round(fee, 2));
        }

        private bool IsLatamCurrency(string currencyCode)
        {
            return Array.Exists(LATAM_CURRENCIES, c => c == currencyCode.ToUpperInvariant());
        }

        private int GetDecimalPlaces(string currencyCode)
        {
            switch (currencyCode.ToUpperInvariant())
            {
                case "JPY":
                case "COP":
                case "CLP":
                    return 0;
                default:
                    return 2;
            }
        }
    }
}
