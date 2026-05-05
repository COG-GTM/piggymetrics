using System.Collections.Generic;
using System.Threading.Tasks;
using PiggyMetrics.FraudDetection.Models;

namespace PiggyMetrics.FraudDetection.Services
{
    public interface IFraudDetectionService
    {
        Task<FraudAlert> AnalyzeTransaction(Transaction transaction);
        Task<IEnumerable<FraudAlert>> GetAlertsByAccount(string accountName);
        Task<FraudAlert> ReviewAlert(string alertId, string reviewedBy, bool legitimate);
        Task<FraudSummary> GetAccountRiskSummary(string accountName);
    }

    public class FraudSummary
    {
        public string AccountName { get; set; }
        public int TotalTransactions { get; set; }
        public int FlaggedTransactions { get; set; }
        public double OverallRiskScore { get; set; }
        public string RiskCategory { get; set; }
    }
}
