using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using SFA.DAS.Providers.Api.Client;

namespace ESFA.DAS.ProvideFeedback.Employer.Application
{
    public class ProviderRefreshService
    {
        private readonly IStoreEmployerEmailDetails _dbRepository;
        private readonly IProviderApiClient _providerApiClient;

        public ProviderRefreshService(IStoreEmployerEmailDetails dbRepository, IProviderApiClient providerApiClient)
        {
            _dbRepository = dbRepository;
            _providerApiClient = providerApiClient;
        }

        public async Task RefreshProviderData()
        {
            var providers = await _providerApiClient.FindAllAsync();
            var cutDownProviders = providers.Select(p => new Provider
            {
                Ukprn = p.Ukprn,
                ProviderName = p.ProviderName
            });

            await _dbRepository.MarkProviderInactive();

            foreach (var provider in cutDownProviders)
            {
                await _dbRepository.UpsertIntoProviders(provider);
            }
        }
    }
}
