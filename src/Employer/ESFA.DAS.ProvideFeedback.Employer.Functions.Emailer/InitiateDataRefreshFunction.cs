using System;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class InitiateDataRefreshFunction
    {
        private readonly IStoreEmployerEmailDetails _dbRepository;

        public InitiateDataRefreshFunction(
            IStoreEmployerEmailDetails dbRepository)
        {
            _dbRepository = dbRepository;
        }

        [FunctionName("InitiateDataRefreshFunction")]
        [return: ServiceBus("%RetrieveProvidersQueueName%", Connection = "ServiceBusConnection")]
        public async Task<string> Run(
            [TimerTrigger("%DataRefreshSchedule%")]TimerInfo myTimer, 
            ILogger log)
        {
            log.LogInformation("Starting invite data refresh.");

            try
            {
                await _dbRepository.ResetFeedback();
                return string.Empty;
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unable to get invite data refresh.");
                throw;
            }
        }
    }
}
