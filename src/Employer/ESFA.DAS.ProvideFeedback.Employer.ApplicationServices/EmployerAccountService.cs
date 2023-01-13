using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Domain.Entities.ApiTypes;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.OuterApi;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.OuterApi.EmployerAccounts;

namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices
{
    public interface IEmployerAccountService
    {
        Task<EmployerUserAccounts> GetUserAccounts(string userId, string email);
    }
    
    public class EmployerAccountService : IEmployerAccountService
    {
        private readonly IOuterApiClient _apiClient;
        
        public EmployerAccountService(IOuterApiClient apiClient)
        {
            _apiClient = apiClient;
        }
        public async Task<EmployerUserAccounts> GetUserAccounts(string userId, string email)
        {
            var result = await _apiClient.Get<GetUserAccountsResponse>(new GetUserAccountsRequest(userId, email));

            return result.Body;
        }
    }
}