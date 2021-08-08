using SFA.DAS.EAS.Account.Api.Types;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.Configuration;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices
{
    public interface ICommitmentService
    {
        Task<GetAllCohortAccountIdsResponse> GetAllCohortAccountIds();
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

        public async Task<GetAllCohortAccountIdsResponse> GetAllCohortAccountIds()
        {
            var baseUrl = GetBaseUrl(_configuration.ApiBaseUrl);
            var url = $"{baseUrl}api/cohorts/accountIds";
            var json = await _httpClient.GetAsync(url);

            return JsonConvert.DeserializeObject<GetAllCohortAccountIdsResponse>(json);
        }
    }
}
