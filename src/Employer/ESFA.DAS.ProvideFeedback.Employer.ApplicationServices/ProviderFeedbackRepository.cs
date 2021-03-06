﻿using System.Threading.Tasks;
using SFA.DAS.Apprenticeships.Api.Types.Providers;
using SFA.DAS.Providers.Api.Client;

namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices
{
    public class ProviderFeedbackRepository : IGetProviderFeedback
    {
        private readonly IProviderApiClient _providerApiClient;

        public ProviderFeedbackRepository(IProviderApiClient providerApiClient)
        {
            _providerApiClient = providerApiClient;
        }

        public async Task<Feedback> GetProviderFeedbackAsync(long ukprn)
        {
            var result = await _providerApiClient.GetAsync(ukprn);
            return result.ProviderFeedback;
        }
    }
}
