using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using PiggyMetrics.Compliance.Models;

namespace PiggyMetrics.Compliance.Repository
{
    public interface IAuditLogRepository
    {
        Task<AuditLog> Create(AuditLog log);
        Task<IEnumerable<AuditLog>> GetByAccount(string accountName, DateTime? from, DateTime? to);
        Task<IEnumerable<AuditLog>> GetByEventType(AuditEventType eventType, int limit);
        Task<long> CountByAccount(string accountName);
    }

    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly IMongoCollection<AuditLog> _logs;

        public AuditLogRepository(IMongoDatabase database)
        {
            _logs = database.GetCollection<AuditLog>("audit_logs");
        }

        public async Task<AuditLog> Create(AuditLog log)
        {
            log.Timestamp = DateTime.UtcNow;
            await _logs.InsertOneAsync(log);
            return log;
        }

        public async Task<IEnumerable<AuditLog>> GetByAccount(string accountName, DateTime? from, DateTime? to)
        {
            var builder = Builders<AuditLog>.Filter;
            var filter = builder.Eq(l => l.AccountName, accountName);

            if (from.HasValue)
                filter = filter & builder.Gte(l => l.Timestamp, from.Value);
            if (to.HasValue)
                filter = filter & builder.Lte(l => l.Timestamp, to.Value);

            return await _logs.Find(filter)
                .SortByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByEventType(AuditEventType eventType, int limit)
        {
            var filter = Builders<AuditLog>.Filter.Eq(l => l.EventType, eventType);
            return await _logs.Find(filter)
                .SortByDescending(l => l.Timestamp)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<long> CountByAccount(string accountName)
        {
            var filter = Builders<AuditLog>.Filter.Eq(l => l.AccountName, accountName);
            return await _logs.CountDocumentsAsync(filter);
        }
    }
}
