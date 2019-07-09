using System;
using System.Linq;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class EmployerFeedbackDataRefreshFunction
    {
        private readonly EmployerFeedbackDataRefresh _inviteDataRefresh;
        private readonly IStoreEmployerEmailDetails _dbRepository;

        public EmployerFeedbackDataRefreshFunction(
            EmployerFeedbackDataRefresh inviteDataRefresh,
            IStoreEmployerEmailDetails dbRepository)
        {
            _inviteDataRefresh = inviteDataRefresh;
            _dbRepository = dbRepository;
        }

        [FunctionName("EmployerFeedbackDataRefreshFunction")]
        public void Run(
            [TimerTrigger("0 0 3 * * MON-FRI", RunOnStartup = true)]TimerInfo myTimer, 
            ILogger log,
            [ServiceBus("data-refresh-messages", Connection = "ServiceBusConnection", EntityType = EntityType.Queue)]IAsyncCollector<EmployerFeedbackRefreshMessage> queue)
        {
            log.LogInformation("Starting invite data refresh.");
            try
            {
                _dbRepository.ResetFeedback();
                var result = _inviteDataRefresh.GetRefreshData();
                log.LogInformation("Finished getting the data from APIs");
                result.AsParallel().ForAll(x => queue.AddAsync(x));
                log.LogInformation("Placed the data in the queue");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unable to get invite data refresh.");
                throw;
            }
        }
    }
}
