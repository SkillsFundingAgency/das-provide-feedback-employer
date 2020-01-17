using System.Collections.Generic;
using System.Threading.Tasks;

namespace ESFA.DAS.EmployerAccounts.Api.Client
{
    public interface IAccountApiClient
    {
        Task<AccountDetailViewModel> GetAccount(string hashedAccountId);
        Task<AccountDetailViewModel> GetAccount(long accountId);
        Task<ICollection<TeamMemberViewModel>> GetAccountUsers(string accountId);
        Task<ICollection<TeamMemberViewModel>> GetAccountUsers(long accountId);
        Task<T> GetResource<T>(string uri);
        Task<ICollection<AccountDetailViewModel>> GetUserAccounts(string userId);
        Task Ping();
    }
}
