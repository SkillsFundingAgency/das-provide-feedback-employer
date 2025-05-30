using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Domain.Entities.ApiTypes;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.OuterApi;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.OuterApi.EmployerAccounts;
using SFA.DAS.GovUK.Auth.Employer;

namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices
{   
    public class EmployerAccountService : IGovAuthEmployerAccountService
    {
        private readonly IOuterApiClient _apiClient;
        
        public EmployerAccountService(IOuterApiClient apiClient)
        {
            _apiClient = apiClient;
        }
        public async Task<EmployerUserAccounts> GetUserAccounts(string userId, string email)
        {
            var result = await _apiClient.Get<GetUserAccountsResponse>(new GetUserAccountsRequest(userId, email));

            return new EmployerUserAccounts
            {
                EmployerAccounts = result.Body.UserAccounts != null
                    ? result.Body.UserAccounts.Select(c => new EmployerUserAccountItem
                    {
                        Role = c.Role,
                        AccountId = c.AccountId,
                        ApprenticeshipEmployerType = Enum.Parse<ApprenticeshipEmployerType>(c.ApprenticeshipEmployerType.ToString()),
                        EmployerName = c.EmployerName,
                    }).ToList()
                    : [],
                FirstName = result.Body.FirstName,
                IsSuspended = result.Body.IsSuspended,
                LastName = result.Body.LastName,
                EmployerUserId = result.Body.EmployerUserId,
            };
        }
    }
}