using System;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class SurveyInviteEmailer
    {
        private readonly EmployerSurveyInviteEmailer _inviteEmailer;

        public SurveyInviteEmailer(EmployerSurveyInviteEmailer inviteEmailer)
        {
            _inviteEmailer = inviteEmailer;
        }

        [FunctionName("SurveyInviteEmailer")]
        public async Task Run(
           // [TimerTrigger("%InviteEmailerSchedule%")]TimerInfo myTimer,
            ILogger log)
        {
            log.LogInformation("Starting employer invite emailer.");

            try
            {
                await _inviteEmailer.SendEmailsAsync();

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
