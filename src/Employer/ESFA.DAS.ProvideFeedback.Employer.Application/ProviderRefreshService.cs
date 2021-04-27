using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;


namespace ESFA.DAS.ProvideFeedback.Employer.Application
{
    public class ProviderRefreshService
    {
        private readonly IStoreEmployerEmailDetails _dbRepository;
        private readonly IRoatpService _roatpService;

        public ProviderRefreshService(IStoreEmployerEmailDetails dbRepository, IRoatpService roatpService)
        {
            _dbRepository = dbRepository;
            _roatpService = roatpService;
        }

        public async Task RefreshProviderData()
        {
            await _dbRepository.MarkProviderInactive();

            var providers = await _roatpService.GetAll();
            var cutDownProviders = providers.Select(p => new Provider
            {
                Ukprn = p.Ukprn,
                ProviderName = p.LegalName
            });

            await _dbRepository.UpsertIntoProviders(cutDownProviders);
        }
    }
}
