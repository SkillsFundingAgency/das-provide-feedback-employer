using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Extensions;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using AspNetCore;

namespace ESFA.DAS.EmployerProvideFeedback.Orchestrators
{
    public class ReviewAnswersOrchestrator
    {
        private readonly IEmployerFeedbackRepository _employerFeedbackRepository;
        private readonly ILogger<ReviewAnswersOrchestrator> _logger;

        public ReviewAnswersOrchestrator(IEmployerFeedbackRepository employerEmailDetailRepository, ILogger<ReviewAnswersOrchestrator> logger)
        {
            _employerFeedbackRepository = employerEmailDetailRepository;
            _logger = logger;
        }

        public async Task SubmitConfirmedEmployerFeedback(SurveyModel surveyModel)
        {
            var employerFeedback = await _employerFeedbackRepository.GetEmployerFeedbackRecord(surveyModel.UserRef, surveyModel.AccountId, surveyModel.Ukprn);
            long feedbackId = 0;
            if(null == employerFeedback)
            {
                feedbackId = await _employerFeedbackRepository.UpsertIntoFeedback(surveyModel.UserRef, surveyModel.AccountId, surveyModel.Ukprn);
            }
            else
            {
                feedbackId = employerFeedback.FeedbackId;
            }

            if (feedbackId == default(long))
            {
                throw new InvalidOperationException($"Unable to find or create feedback record");
            }

            try
            {
                var providerAttributes = await ConvertSurveyToProviderAttributes(surveyModel);

                var feedbackSource = ProvideFeedback.Data.Enums.FeedbackSource.AdHoc;
                if(surveyModel.UniqueCode.HasValue)
                {
                    feedbackSource = ProvideFeedback.Data.Enums.FeedbackSource.Email;
                }

                var employerFeedbackResultId =
                    await _employerFeedbackRepository.CreateEmployerFeedbackResult(
                    feedbackId,
                    surveyModel.Rating.Value.GetDisplayName(),
                    DateTime.UtcNow,
                    feedbackSource,
                    providerAttributes);

                if(null != surveyModel.UniqueCode && surveyModel.UniqueCode.HasValue)
                {
                    // Email journey.
                    await _employerFeedbackRepository.SetCodeBurntDate(surveyModel.UniqueCode.Value);
                }
                else
                {
                    // Ad Hoc journey
                    Guid? uniqueSurveyCode = await _employerFeedbackRepository.GetUniqueSurveyCodeFromFeedbackId(feedbackId);

                    await _employerFeedbackRepository.SetCodeBurntDate(uniqueSurveyCode.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to submit feedback");
            }
        }

        private async Task<IEnumerable<ProviderAttribute>> ConvertSurveyToProviderAttributes(SurveyModel surveyModel)
        {
            var feedbackQuestionAttributes = await _employerFeedbackRepository.GetAllAttributes();
            var providerAttributes = new List<ProviderAttribute>();

            foreach (var attribute in surveyModel.Attributes.Where(s => s.Good || s.Bad))
            {
                var providerAttribute = feedbackQuestionAttributes.FirstOrDefault(s => s.AttributeName == attribute.Name);
                if (providerAttribute != null)
                {
                    providerAttributes.Add(new ProviderAttribute
                    {
                        AttributeId = providerAttribute.AttributeId,
                        AttributeValue = attribute.Score,
                    });
                }
            }

            return providerAttributes;
        }
    }
}
