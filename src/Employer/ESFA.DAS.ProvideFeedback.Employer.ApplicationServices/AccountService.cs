using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.Configuration;
using Newtonsoft.Json;
using SFA.DAS.EAS.Account.Api.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices
{
    public class AccountService : ApplicationService, IAccountService
    {
        private readonly IAccountApiConfiguration _configuration;
        private readonly SecureHttpClient _httpClient;

        public AccountService(IAccountApiConfiguration configuration, SecureHttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<ICollection<TeamMemberViewModel>> GetAccountUsers(long accountId)
        {
            var baseUrl = GetBaseUrl(_configuration.ApiBaseUrl);
            var url = $"{baseUrl}api/accounts/internal/{accountId}/users";
            var json = await _httpClient.GetAsync(url);

            return JsonConvert.DeserializeObject<ICollection<TeamMemberViewModel>>(json);
        }
    }
}
