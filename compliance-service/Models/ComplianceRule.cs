using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PiggyMetrics.Compliance.Models
{
    public class ComplianceRule
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("regulation")]
        public string Regulation { get; set; } = string.Empty;

        [BsonElement("ruleCode")]
        public string RuleCode { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("category")]
        public ComplianceCategory Category { get; set; }

        [BsonElement("severity")]
        public ComplianceSeverity Severity { get; set; }

        [BsonElement("enabled")]
        public bool Enabled { get; set; }

        [BsonElement("lastUpdated")]
        public DateTime LastUpdated { get; set; }

        [BsonElement("jurisdiction")]
        public string? Jurisdiction { get; set; }
    }

    public enum ComplianceCategory
    {
        KYC,
        AML,
        GDPR,
        PSD2,
        DORA,
        MiFID,
        BaselIII
    }

    public enum ComplianceSeverity
    {
        Info,
        Warning,
        Violation,
        Critical
    }
}
