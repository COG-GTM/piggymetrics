using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using PiggyMetrics.FraudDetection.Models;

namespace PiggyMetrics.FraudDetection.Repository
{
    public interface IFraudRuleRepository
    {
        Task<IEnumerable<FraudRule>> GetEnabledRules();
        Task<FraudRule> GetRuleById(string id);
        Task<FraudRule> CreateRule(FraudRule rule);
        Task UpdateRule(string id, FraudRule rule);
        Task DeleteRule(string id);
    }

    public class FraudRuleRepository : IFraudRuleRepository
    {
        private readonly IMongoCollection<FraudRule> _rules;

        public FraudRuleRepository(IMongoDatabase database)
        {
            _rules = database.GetCollection<FraudRule>("fraud_rules");
        }

        public async Task<IEnumerable<FraudRule>> GetEnabledRules()
        {
            var filter = Builders<FraudRule>.Filter.Eq(r => r.Enabled, true);
            return await _rules.Find(filter).ToListAsync();
        }

        public async Task<FraudRule> GetRuleById(string id)
        {
            var filter = Builders<FraudRule>.Filter.Eq(r => r.Id, id);
            return await _rules.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<FraudRule> CreateRule(FraudRule rule)
        {
            rule.CreatedAt = System.DateTime.UtcNow;
            rule.UpdatedAt = System.DateTime.UtcNow;
            await _rules.InsertOneAsync(rule);
            return rule;
        }

        public async Task UpdateRule(string id, FraudRule rule)
        {
            rule.UpdatedAt = System.DateTime.UtcNow;
            var filter = Builders<FraudRule>.Filter.Eq(r => r.Id, id);
            await _rules.ReplaceOneAsync(filter, rule);
        }

        public async Task DeleteRule(string id)
        {
            var filter = Builders<FraudRule>.Filter.Eq(r => r.Id, id);
            await _rules.DeleteOneAsync(filter);
        }
    }
}
