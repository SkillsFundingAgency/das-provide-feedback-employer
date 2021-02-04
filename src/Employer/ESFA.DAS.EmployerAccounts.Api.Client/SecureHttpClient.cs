using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace ESFA.DAS.EmployerAccounts.Api.Client
{
    public class SecureHttpClient
    {
        private readonly IAccountApiConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public SecureHttpClient(HttpClient httpClient, IAccountApiConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }
        protected SecureHttpClient()
        {
            // So we can mock for testing
        }

        private async Task<AuthenticationResult> GetAuthenticationResult(string clientId, string appKey, string resourceId, string tenant)
        {
            var authority = $"https://login.microsoftonline.com/{tenant}";
            var clientCredential = new ClientCredential(clientId, appKey);
            var context = new AuthenticationContext(authority, true);
            var result = await context.AcquireTokenAsync(resourceId, clientCredential);
            return result;
        }

        public virtual async Task<string> GetAsync(string url)
        {
            var authenticationResult = await GetAuthenticationResult(_configuration.ClientId, _configuration.ClientSecret, _configuration.IdentifierUri, _configuration.Tenant);

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
        }
    }
}
