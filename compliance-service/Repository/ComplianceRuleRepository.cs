using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using PiggyMetrics.Compliance.Models;

namespace PiggyMetrics.Compliance.Repository
{
    public interface IComplianceRuleRepository
    {
        Task<IEnumerable<ComplianceRule>> GetActiveRules();
        Task<IEnumerable<ComplianceRule>> GetRulesByCategory(ComplianceCategory category);
        Task<ComplianceRule> GetByRuleCode(string ruleCode);
        Task<ComplianceRule> Create(ComplianceRule rule);
        Task Update(string id, ComplianceRule rule);
    }

    public class ComplianceRuleRepository : IComplianceRuleRepository
    {
        private readonly IMongoCollection<ComplianceRule> _rules;

        public ComplianceRuleRepository(IMongoDatabase database)
        {
            _rules = database.GetCollection<ComplianceRule>("compliance_rules");
        }

        public async Task<IEnumerable<ComplianceRule>> GetActiveRules()
        {
            var filter = Builders<ComplianceRule>.Filter.Eq(r => r.Enabled, true);
            return await _rules.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<ComplianceRule>> GetRulesByCategory(ComplianceCategory category)
        {
            var filter = Builders<ComplianceRule>.Filter.And(
                Builders<ComplianceRule>.Filter.Eq(r => r.Enabled, true),
                Builders<ComplianceRule>.Filter.Eq(r => r.Category, category)
            );
            return await _rules.Find(filter).ToListAsync();
        }

        public async Task<ComplianceRule> GetByRuleCode(string ruleCode)
        {
            var filter = Builders<ComplianceRule>.Filter.Eq(r => r.RuleCode, ruleCode);
            return await _rules.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<ComplianceRule> Create(ComplianceRule rule)
        {
            rule.LastUpdated = System.DateTime.UtcNow;
            await _rules.InsertOneAsync(rule);
            return rule;
        }

        public async Task Update(string id, ComplianceRule rule)
        {
            rule.LastUpdated = System.DateTime.UtcNow;
            var filter = Builders<ComplianceRule>.Filter.Eq(r => r.Id, id);
            await _rules.ReplaceOneAsync(filter, rule);
        }
    }
}
