using System;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public static class SurveyReminderEmailer
    {
        [FunctionName("SurveyReminderEmailer")]
        public static async Task Run(
            [TimerTrigger("0 0 10 * * MON-FRI", RunOnStartup = true)]TimerInfo myTimer,
            [Inject] EmployerSurveyReminderEmailer reminderEmailer,
            ILogger log)
        {
            log.LogInformation("Starting employer invite emailer.");

            try
            {
                await reminderEmailer.SendEmailsAsync();

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
