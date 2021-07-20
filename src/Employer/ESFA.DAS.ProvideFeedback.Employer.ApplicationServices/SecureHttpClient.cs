using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices
{
    public class SecureHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IAzureClientCredentialHelper _azureClientCredentialHelper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IAccountApiConfiguration _configuration;

        public SecureHttpClient(HttpClient httpClient, IAccountApiConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        protected SecureHttpClient()
        {
            // So we can mock for testing
        }

        public virtual async Task<string> GetAsync(string url)
        {
            if (!_hostingEnvironment.IsDevelopment())
            {
                var accessToken = await _azureClientCredentialHelper.GetAccessTokenAsync(_configuration.IdentifierUri);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
