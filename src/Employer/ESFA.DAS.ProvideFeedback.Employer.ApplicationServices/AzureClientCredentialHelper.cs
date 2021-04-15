using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;

namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices
{
    public class AzureClientCredentialHelper : IAzureClientCredentialHelper
    {
        public async Task<string> GetAccessTokenAsync(string identifier)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(identifier);
         
            return accessToken;
        }
    }
}