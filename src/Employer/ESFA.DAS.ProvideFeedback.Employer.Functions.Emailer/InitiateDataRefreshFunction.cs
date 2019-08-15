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
        [return: ServiceBus("retrieve-feedback-data", Connection = "ServiceBusConnection")]
        public async Task<string> Run([TimerTrigger("0 0 3 * * MON-FRI", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
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
