using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PiggyMetrics.FraudDetection.Models
{
    public class Transaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("accountName")]
        public string AccountName { get; set; }

        [BsonElement("amount")]
        public decimal Amount { get; set; }

        [BsonElement("currency")]
        public string Currency { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }

        [BsonElement("type")]
        public TransactionType Type { get; set; }

        [BsonElement("merchantCategory")]
        public string MerchantCategory { get; set; }

        [BsonElement("location")]
        public string Location { get; set; }
    }

    public enum TransactionType
    {
        Income,
        Expense,
        Transfer
    }
}
