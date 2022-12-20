using System;
using System.Net.Http;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.Configuration;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.OuterApi
{
    public class OuterApiClient : IOuterApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly OuterApiConfiguration _config;

        public OuterApiClient(HttpClient httpClient, OuterApiConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
            _httpClient.BaseAddress = new Uri(config.BaseUrl);
        }
        public async Task<ApiResponse<TResponse>> Get<TResponse>(IGetApiRequest request)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, request.GetUrl);
            AddAuthenticationHeader(requestMessage);
            
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

            return await ProcessResponse<TResponse>(response);
        }
        
        private void AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
        {
            httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Key", _config.Key);
            httpRequestMessage.Headers.Add("X-Version", "1");
        }

        private static async Task<ApiResponse<TResponse>> ProcessResponse<TResponse>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            var errorContent = "";
            var responseBody = (TResponse)default;
            
            if(!response.IsSuccessStatusCode)
            {
                errorContent = json;
            }
            else
            {
                responseBody = JsonConvert.DeserializeObject<TResponse>(json);
            }

            var apiResponse = new ApiResponse<TResponse>(responseBody, response.StatusCode, errorContent);
            
            return apiResponse;
        }
    }
}