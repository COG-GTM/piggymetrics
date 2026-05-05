using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using PiggyMetrics.FraudDetection.Models;
using PiggyMetrics.FraudDetection.Repository;

namespace PiggyMetrics.FraudDetection.Services
{
    public class FraudDetectionServiceImpl : IFraudDetectionService
    {
        private readonly IMongoCollection<FraudAlert> _alerts;
        private readonly IFraudRuleRepository _ruleRepository;
        private readonly IAccountServiceClient _accountClient;

        private static readonly decimal HIGH_AMOUNT_THRESHOLD = 10000m;
        private static readonly int VELOCITY_WINDOW_MINUTES = 60;
        private static readonly int VELOCITY_MAX_TRANSACTIONS = 10;

        public FraudDetectionServiceImpl(
            IMongoDatabase database,
            IFraudRuleRepository ruleRepository,
            IAccountServiceClient accountClient)
        {
            _alerts = database.GetCollection<FraudAlert>("fraud_alerts");
            _ruleRepository = ruleRepository;
            _accountClient = accountClient;
        }

        public async Task<FraudAlert> AnalyzeTransaction(Transaction transaction)
        {
            var rules = await _ruleRepository.GetEnabledRules();
            double totalRiskScore = 0;
            var reasons = new List<string>();

            foreach (var rule in rules)
            {
                var ruleResult = EvaluateRule(rule, transaction);
                if (ruleResult.triggered)
                {
                    totalRiskScore += rule.RiskWeight;
                    reasons.Add(ruleResult.reason);
                }
            }

            // Built-in checks
            if (transaction.Amount > HIGH_AMOUNT_THRESHOLD)
            {
                totalRiskScore += 0.3;
                reasons.Add($"High amount transaction: {transaction.Amount} {transaction.Currency}");
            }

            var recentAlerts = await GetRecentAlerts(transaction.AccountName, VELOCITY_WINDOW_MINUTES);
            if (recentAlerts.Count() >= VELOCITY_MAX_TRANSACTIONS)
            {
                totalRiskScore += 0.4;
                reasons.Add($"Velocity check: {recentAlerts.Count()} transactions in last {VELOCITY_WINDOW_MINUTES} minutes");
            }

            var riskLevel = CalculateRiskLevel(totalRiskScore);

            var alert = new FraudAlert
            {
                AccountName = transaction.AccountName,
                TransactionId = transaction.Id,
                RiskScore = Math.Min(totalRiskScore, 1.0),
                RiskLevel = riskLevel,
                Reason = string.Join("; ", reasons),
                CreatedAt = DateTime.UtcNow,
                Reviewed = false
            };

            if (riskLevel != RiskLevel.Low)
            {
                await _alerts.InsertOneAsync(alert);
            }

            return alert;
        }

        public async Task<IEnumerable<FraudAlert>> GetAlertsByAccount(string accountName)
        {
            var filter = Builders<FraudAlert>.Filter.Eq(a => a.AccountName, accountName);
            return await _alerts.Find(filter)
                .SortByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<FraudAlert> ReviewAlert(string alertId, string reviewedBy, bool legitimate)
        {
            var filter = Builders<FraudAlert>.Filter.Eq(a => a.Id, alertId);
            var update = Builders<FraudAlert>.Update
                .Set(a => a.Reviewed, true)
                .Set(a => a.ReviewedBy, reviewedBy);

            await _alerts.UpdateOneAsync(filter, update);

            return await _alerts.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<FraudSummary> GetAccountRiskSummary(string accountName)
        {
            var alerts = await GetAlertsByAccount(accountName);
            var alertList = alerts.ToList();

            return new FraudSummary
            {
                AccountName = accountName,
                TotalTransactions = alertList.Count,
                FlaggedTransactions = alertList.Count(a => a.RiskLevel != RiskLevel.Low),
                OverallRiskScore = alertList.Any() ? alertList.Average(a => a.RiskScore) : 0,
                RiskCategory = alertList.Any(a => a.RiskLevel == RiskLevel.Critical) ? "Critical"
                    : alertList.Any(a => a.RiskLevel == RiskLevel.High) ? "High"
                    : alertList.Any(a => a.RiskLevel == RiskLevel.Medium) ? "Medium"
                    : "Low"
            };
        }

        private (bool triggered, string reason) EvaluateRule(FraudRule rule, Transaction transaction)
        {
            switch (rule.RuleType)
            {
                case RuleType.AmountThreshold:
                    if (transaction.Amount > rule.Threshold)
                        return (true, $"Amount {transaction.Amount} exceeds threshold {rule.Threshold}");
                    break;

                case RuleType.MerchantCategoryRestriction:
                    if (transaction.MerchantCategory == rule.Name)
                        return (true, $"Restricted merchant category: {transaction.MerchantCategory}");
                    break;

                case RuleType.DuplicateTransaction:
                    break;
            }

            return (false, null);
        }

        private async Task<IEnumerable<FraudAlert>> GetRecentAlerts(string accountName, int windowMinutes)
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-windowMinutes);
            var filter = Builders<FraudAlert>.Filter.And(
                Builders<FraudAlert>.Filter.Eq(a => a.AccountName, accountName),
                Builders<FraudAlert>.Filter.Gte(a => a.CreatedAt, cutoff)
            );

            return await _alerts.Find(filter).ToListAsync();
        }

        private RiskLevel CalculateRiskLevel(double score)
        {
            if (score >= 0.8) return RiskLevel.Critical;
            if (score >= 0.6) return RiskLevel.High;
            if (score >= 0.3) return RiskLevel.Medium;
            return RiskLevel.Low;
        }
    }
}
