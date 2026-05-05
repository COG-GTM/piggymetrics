using System.Threading.Tasks;
using RestSharp;

namespace PiggyMetrics.FraudDetection.Services
{
    public interface IAccountServiceClient
    {
        Task<string> GetAccountData(string accountName);
    }

    public class AccountServiceClient : IAccountServiceClient
    {
        private readonly RestClient _client;

        public AccountServiceClient()
        {
            var accountServiceUrl = System.Environment.GetEnvironmentVariable("ACCOUNT_SERVICE_URL")
                ?? "http://account-service:6000";
            _client = new RestClient(accountServiceUrl);
        }

        public async Task<string> GetAccountData(string accountName)
        {
            var request = new RestRequest($"/{accountName}", Method.GET);
            var response = await _client.ExecuteTaskAsync(request);

            if (!response.IsSuccessful)
            {
                throw new System.Exception($"Failed to get account data for {accountName}: {response.StatusCode}");
            }

            return response.Content;
        }
    }
}
