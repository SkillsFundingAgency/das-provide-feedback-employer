using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.Feedback.Employer.Emailer.Models;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Framework.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public static class EmailInviteDataRefreshFunc
    {
        [FunctionName("EmailInviteDataRefresh")]
        public static void Run([TimerTrigger("0 0 3 * * MON-FRI", RunOnStartup = true)]TimerInfo myTimer, [Inject] EmailInviteDataRefresh inviteDataRefresh, ILogger log,
            [ServiceBus("data-refresh-message", Connection = "ServiceBusConnection", EntityType = EntityType.Queue)] IAsyncCollector<DataRefreshMessage> queue, [Inject] DbRepository dbRepository)
        {
            log.LogInformation("Starting invite data refresh.");
            try
            {
                dbRepository.ResetFeedback();
                var result = inviteDataRefresh.GetRefreshData();
                log.LogInformation("Finished getting the data from APIs");
                result.AsParallel().ForAll(x => queue.AddAsync(x));
                log.LogInformation("Placed the data in the DB");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unable to get invite data refresh.");
                throw;
            }
        }
    }
}
