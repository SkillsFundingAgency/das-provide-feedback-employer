using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public static class SurveyReminderEmailer
    {
        [FunctionName("SurveyReminderEmailer")]
        public static async void Run(
            [TimerTrigger("0 0 10 * * MON-FRI", RunOnStartup = true)]TimerInfo myTimer,
            [Inject] EmployerEmailerJob employerEmailerJob,
            ILogger log)
        {
            log.LogInformation("Starting employer emailer job.");
            await employerEmailerJob.EmailEmployerFeedbackInvitations();
        }
    }
}
