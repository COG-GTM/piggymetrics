using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using PiggyMetrics.Compliance.Models;
using PiggyMetrics.Compliance.Repository;

namespace PiggyMetrics.Compliance.Services
{
    public interface IComplianceService
    {
        Task<AuditLog> LogEvent(AuditLog auditLog);
        Task<IEnumerable<AuditLog>> GetAuditTrail(string accountName, DateTime? from, DateTime? to);
        Task<ComplianceReport> RunComplianceCheck(string accountName, DateTime periodStart, DateTime periodEnd);
        Task<IEnumerable<ComplianceRule>> GetActiveRules();
        Task<ComplianceRule> CreateRule(ComplianceRule rule);
    }

    public class ComplianceServiceImpl : IComplianceService
    {
        private readonly IAuditLogRepository _auditRepo;
        private readonly IComplianceRuleRepository _ruleRepo;
        private readonly IMongoCollection<ComplianceReport> _reports;

        private static readonly decimal AML_SINGLE_TRANSACTION_LIMIT = 10000m;
        private static readonly decimal AML_DAILY_AGGREGATE_LIMIT = 25000m;

        public ComplianceServiceImpl(
            IAuditLogRepository auditRepo,
            IComplianceRuleRepository ruleRepo,
            IMongoDatabase database)
        {
            _auditRepo = auditRepo;
            _ruleRepo = ruleRepo;
            _reports = database.GetCollection<ComplianceReport>("compliance_reports");
        }

        public async Task<AuditLog> LogEvent(AuditLog auditLog)
        {
            return await _auditRepo.Create(auditLog);
        }

        public async Task<IEnumerable<AuditLog>> GetAuditTrail(string accountName, DateTime? from, DateTime? to)
        {
            return await _auditRepo.GetByAccount(accountName, from, to);
        }

        public async Task<ComplianceReport> RunComplianceCheck(string accountName, DateTime periodStart, DateTime periodEnd)
        {
            var auditLogs = await _auditRepo.GetByAccount(accountName, periodStart, periodEnd);
            var rules = await _ruleRepo.GetActiveRules();
            var violations = new List<ComplianceViolation>();
            var logList = auditLogs.ToList();

            foreach (var rule in rules)
            {
                var ruleViolations = CheckRule(rule, logList);
                violations.AddRange(ruleViolations);
            }

            // Built-in AML checks
            var transactionLogs = logList
                .Where(l => l.EventType == AuditEventType.TransactionProcessed)
                .ToList();

            foreach (var log in transactionLogs)
            {
                if (log.Metadata != null && log.Metadata.ContainsKey("amount"))
                {
                    if (decimal.TryParse(log.Metadata["amount"], out decimal amount))
                    {
                        if (amount >= AML_SINGLE_TRANSACTION_LIMIT)
                        {
                            violations.Add(new ComplianceViolation
                            {
                                RuleCode = "AML-001",
                                Regulation = "AML",
                                Description = $"Transaction amount {amount} exceeds single transaction reporting threshold",
                                Severity = ComplianceSeverity.Warning,
                                DetectedAt = DateTime.UtcNow,
                                TransactionId = log.Id
                            });
                        }
                    }
                }
            }

            var overallStatus = DetermineStatus(violations);

            var report = new ComplianceReport
            {
                AccountName = accountName,
                GeneratedAt = DateTime.UtcNow,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                OverallStatus = overallStatus,
                Violations = violations,
                TotalTransactionsAudited = transactionLogs.Count,
                RiskScore = CalculateRiskScore(violations)
            };

            await _reports.InsertOneAsync(report);
            return report;
        }

        public async Task<IEnumerable<ComplianceRule>> GetActiveRules()
        {
            return await _ruleRepo.GetActiveRules();
        }

        public async Task<ComplianceRule> CreateRule(ComplianceRule rule)
        {
            return await _ruleRepo.Create(rule);
        }

        private List<ComplianceViolation> CheckRule(ComplianceRule rule, List<AuditLog> logs)
        {
            var violations = new List<ComplianceViolation>();

            switch (rule.Category)
            {
                case ComplianceCategory.GDPR:
                    var dataExports = logs.Where(l => l.EventType == AuditEventType.DataExported).ToList();
                    if (dataExports.Any(e => e.ComplianceFlags == null || !e.ComplianceFlags.Contains("consent_verified")))
                    {
                        violations.Add(new ComplianceViolation
                        {
                            RuleCode = rule.RuleCode,
                            Regulation = rule.Regulation,
                            Description = "Data export without verified consent",
                            Severity = rule.Severity,
                            DetectedAt = DateTime.UtcNow
                        });
                    }
                    break;

                case ComplianceCategory.PSD2:
                    var loginAttempts = logs.Where(l => l.EventType == AuditEventType.LoginAttempt).ToList();
                    if (loginAttempts.Any(l => l.ComplianceFlags == null || !l.ComplianceFlags.Contains("sca_verified")))
                    {
                        violations.Add(new ComplianceViolation
                        {
                            RuleCode = rule.RuleCode,
                            Regulation = rule.Regulation,
                            Description = "Login without Strong Customer Authentication (SCA)",
                            Severity = rule.Severity,
                            DetectedAt = DateTime.UtcNow
                        });
                    }
                    break;
            }

            return violations;
        }

        private ComplianceStatus DetermineStatus(List<ComplianceViolation> violations)
        {
            if (!violations.Any()) return ComplianceStatus.Compliant;
            if (violations.Any(v => v.Severity == ComplianceSeverity.Critical)) return ComplianceStatus.NonCompliant;
            if (violations.Any(v => v.Severity == ComplianceSeverity.Violation)) return ComplianceStatus.MajorViolations;
            return ComplianceStatus.MinorViolations;
        }

        private double CalculateRiskScore(List<ComplianceViolation> violations)
        {
            if (!violations.Any()) return 0;

            double score = 0;
            foreach (var v in violations)
            {
                switch (v.Severity)
                {
                    case ComplianceSeverity.Critical: score += 0.4; break;
                    case ComplianceSeverity.Violation: score += 0.25; break;
                    case ComplianceSeverity.Warning: score += 0.1; break;
                    case ComplianceSeverity.Info: score += 0.02; break;
                }
            }

            return Math.Min(score, 1.0);
        }
    }
}
