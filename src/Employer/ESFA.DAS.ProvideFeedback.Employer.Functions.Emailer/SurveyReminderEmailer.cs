using System;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Framework.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public static class SurveyReminderEmailer
    {
        [FunctionName("SurveyReminderEmailer")]
        public static async Task Run(
            [TimerTrigger("%ReminderEmailerSchedule%", RunOnStartup = true)]TimerInfo myTimer,
            [Inject] EmployerSurveyReminderEmailer reminderEmailer,
            ILogger log)
        {
            log.LogInformation("Starting employer reminder emailer.");

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
