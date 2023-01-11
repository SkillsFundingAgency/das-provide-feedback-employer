using ESFA.DAS.ProvideFeedback.Domain.Entities.ApiTypes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices
{
    public interface IRoatpService
    {
        Task<IEnumerable<ProviderRegistration>> GetAll();
    }
}