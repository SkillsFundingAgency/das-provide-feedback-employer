using System.Collections.Generic;
using System.Data;
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
            await _dbRepository.MarkProviderInactive();

            var providers = await _providerApiClient.FindAllAsync();
            var cutDownProviders = providers.Select(p => new Provider
            {
                Ukprn = p.Ukprn,
                ProviderName = p.ProviderName
            });

            await _dbRepository.UpsertIntoProviders(cutDownProviders);
        }
    }
}
