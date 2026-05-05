using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PiggyMetrics.FraudDetection.Models;
using PiggyMetrics.FraudDetection.Services;

namespace PiggyMetrics.FraudDetection.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FraudController : ControllerBase
    {
        private readonly IFraudDetectionService _fraudService;

        public FraudController(IFraudDetectionService fraudService)
        {
            _fraudService = fraudService;
        }

        /// <summary>
        /// Analyze a transaction for potential fraud
        /// </summary>
        [HttpPost("analyze")]
        public async Task<ActionResult<FraudAlert>> AnalyzeTransaction([FromBody] Transaction transaction)
        {
            if (transaction == null)
                return BadRequest("Transaction cannot be null");

            var alert = await _fraudService.AnalyzeTransaction(transaction);
            return Ok(alert);
        }

        /// <summary>
        /// Get all fraud alerts for an account
        /// </summary>
        [HttpGet("alerts/{accountName}")]
        public async Task<IActionResult> GetAlerts(string accountName)
        {
            var alerts = await _fraudService.GetAlertsByAccount(accountName);
            return Ok(alerts);
        }

        /// <summary>
        /// Review a fraud alert
        /// </summary>
        [HttpPut("alerts/{alertId}/review")]
        public async Task<IActionResult> ReviewAlert(string alertId, [FromQuery] string reviewedBy, [FromQuery] bool legitimate)
        {
            var alert = await _fraudService.ReviewAlert(alertId, reviewedBy, legitimate);
            if (alert == null)
                return NotFound();

            return Ok(alert);
        }

        /// <summary>
        /// Get risk summary for an account
        /// </summary>
        [HttpGet("risk/{accountName}")]
        public async Task<ActionResult<FraudSummary>> GetRiskSummary(string accountName)
        {
            var summary = await _fraudService.GetAccountRiskSummary(accountName);
            return Ok(summary);
        }
    }
}
