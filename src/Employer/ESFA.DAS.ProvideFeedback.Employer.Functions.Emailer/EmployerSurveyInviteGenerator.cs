using System;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Framework.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public static class EmployerSurveyInviteGenerator
    {
        [FunctionName("EmployerSurveyInviteGenerator")]
        public static async Task Run(
            [TimerTrigger("3 3 * * *")]TimerInfo myTimer,
            [Inject] IOptions<EmailSettings> options,
            [Inject] IStoreEmployerEmailDetails employerEmailDetailRepository, 
            ILogger log)
        {
            log.LogInformation($"Employer Survey Invite generator executed at: {DateTime.Now}");

            var newCodesRequired = await employerEmailDetailRepository.GetEmployerInvitesForNextCycleAsync(options.Value.InviteCycleDays);

            await employerEmailDetailRepository.InsertNewSurveyInviteCodes(newCodesRequired);

            log.LogInformation($"Employer Survey Invite generator completed at: {DateTime.Now}");
        }
    }
}
