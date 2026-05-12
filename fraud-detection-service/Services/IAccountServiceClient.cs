using System.Net.Http;
using System.Threading.Tasks;

namespace PiggyMetrics.FraudDetection.Services
{
    public interface IAccountServiceClient
    {
        Task<string> GetAccountData(string accountName);
    }

    public class AccountServiceClient : IAccountServiceClient
    {
        private readonly HttpClient _httpClient;

        public AccountServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetAccountData(string accountName)
        {
            var response = await _httpClient.GetAsync($"/{accountName}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
