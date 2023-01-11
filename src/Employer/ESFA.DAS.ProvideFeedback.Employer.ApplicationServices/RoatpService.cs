using ESFA.DAS.ProvideFeedback.Domain.Entities.ApiTypes;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices
{
    public class RoatpService : IRoatpService
    {
        private readonly HttpClient _httpClient;
        private readonly IAzureClientCredentialHelper _azureClientCredentialHelper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly RoatpApiConfiguration _configuration;

        public RoatpService(HttpClient httpClient, IAzureClientCredentialHelper azureClientCredentialHelper, IHostingEnvironment hostingEnvironment, IOptions<RoatpApiConfiguration> configuration)
        {
            _configuration = configuration.Value;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_configuration.Url);
            _azureClientCredentialHelper = azureClientCredentialHelper;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<IEnumerable<ProviderRegistration>> GetAll()
        {
            await AddAuthenticationHeader();

            var response = await _httpClient.GetAsync("v1/fat-data-export");

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<ProviderRegistration>>(jsonResponse);
        }

        private async Task AddAuthenticationHeader()
        {
            if (!_hostingEnvironment.IsDevelopment())
            {
                var accessToken = await _azureClientCredentialHelper.GetAccessTokenAsync(_configuration.Identifier);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }
    }
}