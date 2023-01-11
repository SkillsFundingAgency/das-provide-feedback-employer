﻿using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Employer.Application
{
    public class SurveyInviteGenerator
    {
        private readonly EmailSettings _emailSettingsConfig;
        private readonly IEmployerFeedbackRepository _employerEmailDetailRepository;
        private readonly ILogger<SurveyInviteGenerator> _logger;

        public SurveyInviteGenerator(
            IOptions<EmailSettings> options,
            IEmployerFeedbackRepository employerEmailDetailRepository,
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
                _logger.LogInformation($"Generated New Survey Code for feedbackId : {feedbackId} ");
            }
        }

        private async Task<bool> IsNewSurveyCodeRequired(long feedbackId)
        {
            var minimumLastReminderDate = DateTime.UtcNow.AddDays(-_emailSettingsConfig.InviteCycleDays);
            var feedbackLastSent = await _employerEmailDetailRepository.GetLatestFeedbackInviteSentDateAsync(feedbackId);
            if (feedbackLastSent is null) return false;
            _logger.LogInformation($"FeedbackId : {feedbackId}  UniqueSurveyCode : {feedbackLastSent.UniqueSurveyCode}  InviteSentDate : {feedbackLastSent.InviteSentDate}  MinimumLastReminderDate : {minimumLastReminderDate}");
            return feedbackLastSent.UniqueSurveyCode == null ||
                (feedbackLastSent.InviteSentDate != null && feedbackLastSent.InviteSentDate < minimumLastReminderDate);
        }
    }
}
