using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PiggyMetrics.CurrencyExchange.Models;
using PiggyMetrics.CurrencyExchange.Services;

namespace PiggyMetrics.CurrencyExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyConversionService _conversionService;
        private readonly IExchangeRateProvider _rateProvider;

        public CurrencyController(
            ICurrencyConversionService conversionService,
            IExchangeRateProvider rateProvider)
        {
            _conversionService = conversionService;
            _rateProvider = rateProvider;
        }

        /// <summary>
        /// Convert currency
        /// </summary>
        [HttpPost("convert")]
        public async Task<ActionResult<ConversionResult>> Convert([FromBody] ConversionRequest request)
        {
            try
            {
                var result = await _conversionService.Convert(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Conversion failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Get latest exchange rates for a base currency
        /// </summary>
        [HttpGet("rates/{baseCurrency}")]
        public async Task<ActionResult<ExchangeRateTable>> GetRates(string baseCurrency)
        {
            try
            {
                var rates = await _rateProvider.GetLatestRates(baseCurrency.ToUpperInvariant());
                return Ok(rates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to fetch rates: {ex.Message}");
            }
        }

        /// <summary>
        /// Get exchange rate between two currencies
        /// </summary>
        [HttpGet("rate/{baseCurrency}/{targetCurrency}")]
        public async Task<ActionResult<ExchangeRate>> GetRate(string baseCurrency, string targetCurrency)
        {
            try
            {
                var rate = await _rateProvider.GetRate(
                    baseCurrency.ToUpperInvariant(),
                    targetCurrency.ToUpperInvariant());
                return Ok(rate);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Rate not found for {baseCurrency}/{targetCurrency}");
            }
        }

        /// <summary>
        /// Get supported currencies
        /// </summary>
        [HttpGet("currencies")]
        public IActionResult GetSupportedCurrencies()
        {
            return Ok(_rateProvider.GetSupportedCurrencies());
        }

        /// <summary>
        /// Calculate conversion fee
        /// </summary>
        [HttpGet("fee")]
        public async Task<IActionResult> GetFee(
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] decimal amount)
        {
            var fee = await _conversionService.GetConversionFee(from, to, amount);
            return Ok(new { from, to, amount, fee });
        }
    }
}
