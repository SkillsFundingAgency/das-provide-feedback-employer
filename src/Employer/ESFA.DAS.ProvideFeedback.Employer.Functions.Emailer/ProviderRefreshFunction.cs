using ESFA.DAS.ProvideFeedback.Employer.Application;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class ProviderRefreshFunction
    {
        private ProviderRefreshService _providerRefreshService;

        public ProviderRefreshFunction(ProviderRefreshService providerRefreshService)
        {
            _providerRefreshService = providerRefreshService;
        }

        [FunctionName("ProviderRefreshFunction")]
        [return: ServiceBus("%RetrieveFeedbackAccountsQueueName%", Connection = "ServiceBusConnection")]
        public async Task<string> Run(
            [ServiceBusTrigger("%RetrieveProvidersQueueName%", Connection = "ServiceBusConnection")] string myQueueItem,
            ILogger log)
        {
            log.LogInformation($"Provider Refresh function started");

            try
            {
                await _providerRefreshService.RefreshProviderData();
                return string.Empty;
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to refresh providers");
                throw;
            }
        }
    }
}
