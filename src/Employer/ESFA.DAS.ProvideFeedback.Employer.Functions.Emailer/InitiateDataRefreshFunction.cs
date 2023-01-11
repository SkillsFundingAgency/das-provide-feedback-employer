using ESFA.DAS.ProvideFeedback.Data.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class InitiateDataRefreshFunction
    {
        private readonly IEmployerFeedbackRepository _dbRepository;

        public InitiateDataRefreshFunction(
            IEmployerFeedbackRepository dbRepository)
        {
            _dbRepository = dbRepository;
        }

        [FunctionName("InitiateDataRefreshFunction")]
        [return: ServiceBus("%RetrieveProvidersQueueName%", Connection = "ServiceBusConnection")]
        public async Task<string> Run(
            [TimerTrigger("%DataRefreshSchedule%")] TimerInfo myTimer,
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
