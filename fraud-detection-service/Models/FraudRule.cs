using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PiggyMetrics.FraudDetection.Models
{
    public class FraudRule
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("ruleType")]
        public RuleType RuleType { get; set; }

        [BsonElement("threshold")]
        public decimal Threshold { get; set; }

        [BsonElement("riskWeight")]
        public double RiskWeight { get; set; }

        [BsonElement("enabled")]
        public bool Enabled { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }

    public enum RuleType
    {
        AmountThreshold,
        VelocityCheck,
        GeographicAnomaly,
        MerchantCategoryRestriction,
        TimeBasedPattern,
        DuplicateTransaction
    }
}
