using System;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class EmployerSurveyInviteGenerator
    {
        private readonly EmailSettings _emailSettingsConfig;
        private readonly IStoreEmployerEmailDetails _employerEmailDetailRepository;

        public EmployerSurveyInviteGenerator(
            IOptions<EmailSettings> options,
             IStoreEmployerEmailDetails employerEmailDetailRepository)
        {
            _emailSettingsConfig = options.Value;
            _employerEmailDetailRepository = employerEmailDetailRepository;
        }

        [FunctionName("EmployerSurveyInviteGenerator")]
        public async Task Run(
            [TimerTrigger("3 3 * * *")]TimerInfo myTimer,
            ILogger log)
        {
            log.LogInformation($"Employer Survey Invite generator executed at: {DateTime.Now}");

            var newCodesRequired = await _employerEmailDetailRepository.GetEmployerInvitesForNextCycleAsync(_emailSettingsConfig.InviteCycleDays);

            await _employerEmailDetailRepository.InsertNewSurveyInviteCodes(newCodesRequired);

            log.LogInformation($"Employer Survey Invite generator completed at: {DateTime.Now}");
        }
    }
}
