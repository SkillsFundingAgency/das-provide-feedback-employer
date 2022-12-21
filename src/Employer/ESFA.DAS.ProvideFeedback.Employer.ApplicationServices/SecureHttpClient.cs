using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices
{
    public class SecureHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IAzureClientCredentialHelper _azureClientCredentialHelper;

        public SecureHttpClient(HttpClient httpClient, IAzureClientCredentialHelper azureClientCredentialHelper)
        {
            _httpClient = httpClient;
            _azureClientCredentialHelper = azureClientCredentialHelper;
        }

        protected SecureHttpClient()
        {
            // So we can mock for testing
        }

        public virtual async Task<string> GetAsync(string url, string apiTokenIdentifierUri)
        {
            if (!string.IsNullOrEmpty(apiTokenIdentifierUri))
            {
                var accessToken = await _azureClientCredentialHelper.GetAccessTokenAsync(apiTokenIdentifierUri);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
