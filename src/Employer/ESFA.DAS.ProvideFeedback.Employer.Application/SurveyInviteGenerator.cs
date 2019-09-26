using System;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESFA.DAS.ProvideFeedback.Employer.Application
{
    public class SurveyInviteGenerator
    {
        private readonly EmailSettings _emailSettingsConfig;
        private readonly IStoreEmployerEmailDetails _employerEmailDetailRepository;
        private readonly ILogger<SurveyInviteGenerator> _logger;

        public SurveyInviteGenerator(
            IOptions<EmailSettings> options,
            IStoreEmployerEmailDetails employerEmailDetailRepository,
            ILogger<SurveyInviteGenerator> logger)
        {
            _emailSettingsConfig = options.Value;
            _employerEmailDetailRepository = employerEmailDetailRepository;
            _logger = logger;
        }

        public async Task GenerateSurveyInvites(GenerateSurveyCodeMessage message)
        {
            var feedbackId = await _employerEmailDetailRepository.UpsertIntoFeedback(message.UserRef, message.AccountId, message.Ukprn);
            _logger.LogInformation("Done upserting feedback");

            if (await IsNewSurveyCodeRequired(feedbackId))
            {
                await _employerEmailDetailRepository.InsertNewSurveyForFeedback(feedbackId);
            }
        }

        private async Task<bool> IsNewSurveyCodeRequired(long feedbackId)
        {
            var feedbackLastSent = await _employerEmailDetailRepository.GetEmployerSurveyInvite(feedbackId);
            return feedbackLastSent == null || feedbackLastSent.InviteSentDate < DateTime.UtcNow.AddDays(-_emailSettingsConfig.InviteCycleDays);
        }
    }
}
