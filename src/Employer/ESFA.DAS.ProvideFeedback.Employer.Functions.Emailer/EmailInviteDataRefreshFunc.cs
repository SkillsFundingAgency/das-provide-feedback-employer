using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Framework.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public static class EmailInviteDataRefreshFunc
    {
        [FunctionName("EmailInviteDataRefresh")]
        public static async Task Run([TimerTrigger("0 0 3 * * MON-FRI",RunOnStartup = true)]TimerInfo myTimer, [Inject] EmailInviteDataRefresh inviteDataRefresh, ILogger log
            , [Inject] DbRepository dbRepository)
        {
            log.LogInformation("Starting invite data refresh.");

            try
            {
                dbRepository.ResetFeedback();
                var result = inviteDataRefresh.GetRefreshData();
                log.LogInformation("Finished getting the data from APIs");
                result.AsParallel().ForAll(x => dbRepository.ReceiveDataRefreshMessage(x));
                log.LogInformation("Placed the data in the DB");
                var afterDB =  await dbRepository.GetFeedbackToSendResponses();
                log.LogInformation("Finished invite data refresh.");
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Unable to get invite data refresh.");
                throw;
            }
        }
    }
}
