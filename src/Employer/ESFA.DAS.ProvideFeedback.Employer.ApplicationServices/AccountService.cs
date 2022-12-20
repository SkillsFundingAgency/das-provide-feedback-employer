using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.Configuration;
using Newtonsoft.Json;
using SFA.DAS.EAS.Account.Api.Types;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Domain.Entities.ApiTypes;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.OuterApi;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.OuterApi.EmployerAccounts;

namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices
{
    public class AccountService : ApplicationService, IAccountService
    {
        private readonly IAccountApiConfiguration _configuration;
        private readonly SecureHttpClient _httpClient;
        private readonly IOuterApiClient _apiClient;

        public AccountService(IAccountApiConfiguration configuration, SecureHttpClient httpClient, IOuterApiClient apiClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _apiClient = apiClient;
        }

        public async Task<ICollection<TeamMemberViewModel>> GetAccountUsers(long accountId)
        {
            var baseUrl = GetBaseUrl(_configuration.ApiBaseUrl);
            var url = $"{baseUrl}api/accounts/internal/{accountId}/users";
            var json = await _httpClient.GetAsync(url, _configuration.IdentifierUri);

            return JsonConvert.DeserializeObject<ICollection<TeamMemberViewModel>>(json);
        }
        
        public async Task<EmployerUserAccounts> GetUserAccounts(string userId, string email)
        {
            var result = await _apiClient.Get<GetUserAccountsResponse>(new GetUserAccountsRequest(userId, email));

            return result.Body;
        }
    }
}
