using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PiggyMetrics.FraudDetection.Models
{
    public class FraudAlert
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("accountName")]
        public string AccountName { get; set; }

        [BsonElement("transactionId")]
        public string TransactionId { get; set; }

        [BsonElement("riskScore")]
        public double RiskScore { get; set; }

        [BsonElement("riskLevel")]
        public RiskLevel RiskLevel { get; set; }

        [BsonElement("reason")]
        public string Reason { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("reviewed")]
        public bool Reviewed { get; set; }

        [BsonElement("reviewedBy")]
        public string ReviewedBy { get; set; }
    }

    public enum RiskLevel
    {
        Low,
        Medium,
        High,
        Critical
    }
}
