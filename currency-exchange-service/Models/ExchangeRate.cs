using System;
using System.Collections.Generic;

namespace PiggyMetrics.CurrencyExchange.Models
{
    public class ExchangeRate
    {
        public string BaseCurrency { get; set; }
        public string TargetCurrency { get; set; }
        public decimal Rate { get; set; }
        public DateTime Timestamp { get; set; }
        public string Source { get; set; }
    }

    public class ExchangeRateTable
    {
        public string BaseCurrency { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, decimal> Rates { get; set; }
    }

    public class ConversionRequest
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
    }

    public class ConversionResult
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Fee { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class SupportedCurrency
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public int DecimalPlaces { get; set; }
    }
}
