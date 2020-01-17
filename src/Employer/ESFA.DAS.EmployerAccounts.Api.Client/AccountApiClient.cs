using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ESFA.DAS.EmployerAccounts.Api.Client
{
    public class AccountApiClient : IAccountApiClient
    {
        private readonly IAccountApiConfiguration _configuration;
        private readonly SecureHttpClient _httpClient;

        public AccountApiClient(IAccountApiConfiguration configuration, SecureHttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<AccountDetailViewModel> GetAccount(string hashedAccountId)
        {
            var baseUrl = GetBaseUrl();
            var url = $"{baseUrl}api/accounts/{hashedAccountId}";
            var json = await _httpClient.GetAsync(url);

            return JsonConvert.DeserializeObject<AccountDetailViewModel>(json);
        }

        public async Task<AccountDetailViewModel> GetAccount(long accountId)
        {
            var baseUrl = GetBaseUrl();
            var url = $"{baseUrl}api/accounts/internal/{accountId}";
            var json = await _httpClient.GetAsync(url);

            return JsonConvert.DeserializeObject<AccountDetailViewModel>(json);
        }

        public async Task<ICollection<TeamMemberViewModel>> GetAccountUsers(string accountId)
        {
            var baseUrl = GetBaseUrl();
            var url = $"{baseUrl}api/accounts/{accountId}/users";
            var json = await _httpClient.GetAsync(url);

            return JsonConvert.DeserializeObject<ICollection<TeamMemberViewModel>>(json);
        }

        public async Task<ICollection<TeamMemberViewModel>> GetAccountUsers(long accountId)
        {
            var baseUrl = GetBaseUrl();
            var url = $"{baseUrl}api/accounts/internal/{accountId}/users";
            var json = await _httpClient.GetAsync(url);

            return JsonConvert.DeserializeObject<ICollection<TeamMemberViewModel>>(json);
        }

        public async Task<T> GetResource<T>(string uri)
        {
            var absoluteUri = new Uri(new Uri(GetBaseUrl()), uri);
            var json = await _httpClient.GetAsync(absoluteUri.ToString());

            return JsonConvert.DeserializeObject<T>(json);
        }
        
        public async Task<ICollection<AccountDetailViewModel>> GetUserAccounts(string userId)
        {
            var baseUrl = GetBaseUrl();
            var url = $"{baseUrl}api/user/{userId}/accounts";
            var json = await _httpClient.GetAsync(url);

            return JsonConvert.DeserializeObject<ICollection<AccountDetailViewModel>>(json);
        }

        public Task Ping()
        {
            var baseUrl = GetBaseUrl();
            var url = $"{baseUrl}api/ping";

            return _httpClient.GetAsync(url);
        }

        private string GetBaseUrl()
        {
            return _configuration.ApiBaseUrl.EndsWith("/")
                ? _configuration.ApiBaseUrl
                : _configuration.ApiBaseUrl + "/";
        }
    }
}
