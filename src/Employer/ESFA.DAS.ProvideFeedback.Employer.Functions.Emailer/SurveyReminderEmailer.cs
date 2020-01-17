using System;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class SurveyReminderEmailer
    {
        private readonly EmployerSurveyReminderEmailer _reminderEmailer;

        public SurveyReminderEmailer(EmployerSurveyReminderEmailer reminderEmailer)
        {
            _reminderEmailer = reminderEmailer;
        }

        [FunctionName("SurveyReminderEmailer")]
        public async Task Run(
            [TimerTrigger("%ReminderEmailerSchedule%")]TimerInfo myTimer,
            ILogger log)
        {
            log.LogInformation("Starting employer reminder emailer.");

            try
            {
                await _reminderEmailer.SendEmailsAsync();

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
