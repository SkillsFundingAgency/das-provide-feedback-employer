using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.Configuration;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices
{
    public interface ICommitmentService
    {
        Task<GetApprenticeshipsResponse> GetApprenticeships(long accountId);
        Task<GetProviderResponse> GetProvider(long providerId);
    }

    public class CommitmentService : ApplicationService, ICommitmentService
    {
        private readonly ICommitmentApiConfiguration _configuration;
        private readonly SecureHttpClient _httpClient;

        public CommitmentService(ICommitmentApiConfiguration configuration, SecureHttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<GetApprenticeshipsResponse> GetApprenticeships(long accountId)
        {
            var baseUrl = GetBaseUrl(_configuration.ApiBaseUrl);

            var url = $"{baseUrl}api/apprenticeships/?accountId={accountId}&pageNumber=0&pageItemCount={int.MaxValue}";
            var json = await _httpClient.GetAsync(url, _configuration.IdentifierUri);

            return JsonConvert.DeserializeObject<GetApprenticeshipsResponse>(json);
        }

        public async Task<GetProviderResponse> GetProvider(long providerId)
        {
            var baseUrl = GetBaseUrl(_configuration.ApiBaseUrl);

            var url = $"{baseUrl}api/providers/{providerId}";
            var json = await _httpClient.GetAsync(url, _configuration.IdentifierUri);

            return JsonConvert.DeserializeObject<GetProviderResponse>(json);
        }
    }
}
