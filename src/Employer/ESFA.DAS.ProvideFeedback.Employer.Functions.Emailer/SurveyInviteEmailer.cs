using System;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public static class SurveyInviteEmailer
    {
        [FunctionName("SurveyInviteEmailer")]
        public static async void Run(
            [TimerTrigger("0 0 10 * * MON-FRI")]TimerInfo myTimer,
            [Inject] EmployerSurveyInviteEmailer inviteEmailer,
            ILogger log)
        {
            log.LogInformation("Starting employer invite emailer.");

            try
            {
                await inviteEmailer.SendEmailsAsync();

                log.LogInformation("Finished emailing employers.");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unable to email employers.");
                throw;
            }
        }
    }
}