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
        private readonly ILogger<InitiateDataRefreshFunction> _logger;

        public InitiateDataRefreshFunction(
            IStoreEmployerEmailDetails dbRepository, 
            ILogger<InitiateDataRefreshFunction> logger)
        {
            _dbRepository = dbRepository;
            _logger = logger;
        }

        [FunctionName("EmployerFeedbackDataRefreshFunction")]
        [return: ServiceBus("%RetrieveFeedbackDataMessageQueueName%", Connection = "ServiceBusConnection")]
        public async Task<string> Run([TimerTrigger("%DataRefreshSchedule%", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            _logger.LogInformation("Starting invite data refresh.");
            try
            {
                await _dbRepository.ResetFeedback();
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get invite data refresh.");
                throw;
            }
        }
    }
}
