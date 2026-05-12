using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PiggyMetrics.Compliance.Models
{
    public class ComplianceReport
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("accountName")]
        public string AccountName { get; set; } = string.Empty;

        [BsonElement("generatedAt")]
        public DateTime GeneratedAt { get; set; }

        [BsonElement("periodStart")]
        public DateTime PeriodStart { get; set; }

        [BsonElement("periodEnd")]
        public DateTime PeriodEnd { get; set; }

        [BsonElement("overallStatus")]
        public ComplianceStatus OverallStatus { get; set; }

        [BsonElement("violations")]
        public List<ComplianceViolation> Violations { get; set; } = new();

        [BsonElement("totalTransactionsAudited")]
        public int TotalTransactionsAudited { get; set; }

        [BsonElement("riskScore")]
        public double RiskScore { get; set; }
    }

    public class ComplianceViolation
    {
        [BsonElement("ruleCode")]
        public string RuleCode { get; set; } = string.Empty;

        [BsonElement("regulation")]
        public string Regulation { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("severity")]
        public ComplianceSeverity Severity { get; set; }

        [BsonElement("detectedAt")]
        public DateTime DetectedAt { get; set; }

        [BsonElement("transactionId")]
        public string? TransactionId { get; set; }
    }

    public enum ComplianceStatus
    {
        Compliant,
        MinorViolations,
        MajorViolations,
        NonCompliant
    }
}
