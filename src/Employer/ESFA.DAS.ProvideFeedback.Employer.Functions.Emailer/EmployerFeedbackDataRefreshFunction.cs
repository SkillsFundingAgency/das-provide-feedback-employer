using System;
using System.Linq;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Framework.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public static class EmployerFeedbackDataRefreshFunction
    {
        [FunctionName("EmployerFeedbackDataRefreshFunction")]
        public static void Run(
            [TimerTrigger("0 0 3 * * MON-FRI", RunOnStartup = true)]TimerInfo myTimer, 
            [Inject] EmployerFeedbackDataRefresh inviteDataRefresh, 
            ILogger log,
            [ServiceBus("data-refresh-messages", Connection = "ServiceBusConnection", EntityType = EntityType.Queue)]IAsyncCollector<EmployerFeedbackRefreshMessage> queue, 
            [Inject] IStoreEmployerEmailDetails dbRepository)
        {
            log.LogInformation("Starting invite data refresh.");
            try
            {
                dbRepository.ResetFeedback();
                var result = inviteDataRefresh.GetRefreshData();
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
