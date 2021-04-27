using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Domain.Entities.ApiTypes;

namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices
{
    public interface IRoatpService
    {
        Task<IEnumerable<ProviderRegistration>> GetAll();
    }
}