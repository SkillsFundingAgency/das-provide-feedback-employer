using System;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class EmployerSurveyInviteGeneratorFunction
    {
        private readonly EmailSettings _emailSettingsConfig;
        private readonly IStoreEmployerEmailDetails _employerEmailDetailRepository;

        public EmployerSurveyInviteGeneratorFunction(
            IOptions<EmailSettings> options,
            IStoreEmployerEmailDetails employerEmailDetailRepository)
        {
            _emailSettingsConfig = options.Value;
            _employerEmailDetailRepository = employerEmailDetailRepository;
        }

        [FunctionName("EmployerSurveyInviteGenerator")]
        public async Task Run(
            [ServiceBusTrigger("generate-survey-invite", Connection = "ServiceBusConnection")]string feedbackForCodeGeneration,
            ILogger log)
        {
            log.LogInformation($"Employer Survey Invite generator executed at: {DateTime.Now}");

            var message = JsonConvert.DeserializeObject<GenerateSurveyCodeMessage>(feedbackForCodeGeneration);
            var feedbackId = await _employerEmailDetailRepository.UpsertIntoFeedback(message.UserRef, message.AccountId, message.Ukprn);
            log.LogInformation("Done upserting feedback");

            if (await IsNewSurveyCodeRequired(feedbackId))
            {
                await _employerEmailDetailRepository.InsertNewSurveyForFeedback(feedbackId);
            }

            log.LogInformation($"Employer Survey Invite generator completed at: {DateTime.Now}");
        }

        private async Task<bool> IsNewSurveyCodeRequired(long feedbackId)
        {
            var feedbackLastSent = await _employerEmailDetailRepository.GetEmployerSurveyInvite(feedbackId);
            return feedbackLastSent == null || feedbackLastSent.InviteSentDate < DateTime.UtcNow.AddDays(-_emailSettingsConfig.InviteCycleDays);
        }
    }
}
