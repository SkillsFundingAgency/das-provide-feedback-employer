using SFA.DAS.EAS.Account.Api.Types;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;

namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices
{
    public interface IAccountService
    {
        Task<ICollection<TeamMemberViewModel>> GetAccountUsers(long accountId);
    }
}
