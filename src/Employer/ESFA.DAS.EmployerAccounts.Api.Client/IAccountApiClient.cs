using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EAS.Account.Api.Types;

namespace ESFA.DAS.EmployerAccounts.Api.Client
{
    public interface IAccountApiClient
    {
        Task<ICollection<TeamMemberViewModel>> GetAccountUsers(long accountId);
    }
}
