namespace PiggyMetrics.CurrencyExchange.Models
{
    public class ExchangeRate
    {
        public required string BaseCurrency { get; set; }
        public required string TargetCurrency { get; set; }
        public decimal Rate { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Source { get; set; }
    }

    public class ExchangeRateTable
    {
        public required string BaseCurrency { get; set; }
        public DateTime Timestamp { get; set; }
        public required Dictionary<string, decimal> Rates { get; set; }
    }

    public class ConversionRequest
    {
        public required string FromCurrency { get; set; }
        public required string ToCurrency { get; set; }
        public decimal Amount { get; set; }
    }

    public class ConversionResult
    {
        public required string FromCurrency { get; set; }
        public required string ToCurrency { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Fee { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class SupportedCurrency
    {
        public required string Code { get; set; }
        public required string Name { get; set; }
        public required string Symbol { get; set; }
        public int DecimalPlaces { get; set; }
    }
}
