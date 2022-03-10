using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Extensions;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.FeedbackDataAccess.Repositories;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using IEmployerFeedbackRepositorySql = ESFA.DAS.ProvideFeedback.Data.Repositories.IEmployerFeedbackRepository;
using System.Collections.Generic;
using ESFA.DAS.FeedbackDataAccess.Models;

namespace ESFA.DAS.EmployerProvideFeedback.Orchestrators
{
    public class ReviewAnswersOrchestrator
    {
        private readonly IEmployerFeedbackRepository _employerFeedbackRepository;
        private readonly IEmployerFeedbackRepositorySql _employerEmailDetailRepository;
        private readonly ILogger<ReviewAnswersOrchestrator> _logger;

        public ReviewAnswersOrchestrator(IEmployerFeedbackRepository employerFeedbackRepository, IEmployerFeedbackRepositorySql employerEmailDetailRepository, ILogger<ReviewAnswersOrchestrator> logger)
        {
            _employerFeedbackRepository = employerFeedbackRepository;
            _employerEmailDetailRepository = employerEmailDetailRepository;
            _logger = logger;
        }

        // Cosmos Orchestration

        public async Task SubmitConfirmedEmployerFeedback(SurveyModel surveyModel, Guid uniqueCode)
        {
            var now = DateTime.UtcNow;
            await SubmitConfirmedEmployerFeedbackCosmos(surveyModel, uniqueCode, now);
            await SubmitConfirmedEmployerFeedbackSql(surveyModel, uniqueCode, now);
        }
        public async Task SubmitConfirmedEmployerFeedbackCosmos(SurveyModel surveyModel, Guid uniqueCode, DateTime now)
        {
            var employerFeedback = ConvertToEmployerFeedback(surveyModel, now);
            Document doc = null;

            try
            {
                doc = await _employerFeedbackRepository.CreateItemAsync(employerFeedback);
                await _employerEmailDetailRepository.SetCodeBurntDate(uniqueCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to submit feedback");
            }
        }

        private EmployerFeedback ConvertToEmployerFeedback(SurveyModel survey, DateTime now)
        {
            return new EmployerFeedback
            {
                Id = Guid.NewGuid(),
                UserRef = survey.UserRef,
                AccountId = survey.AccountId,
                DateTimeCompleted = now,
                Ukprn = survey.Ukprn,
                ProviderAttributes = survey.Attributes.Select(ps =>
                {
                    return new ProviderAttribute
                    {
                        Name = ps.Name,
                        Value = ps.Score
                    };

                }).ToList(),
                ProviderRating = survey.Rating.Value.GetDisplayName()
            };
        }

        // SQL Orchestration

        public async Task SubmitConfirmedEmployerFeedbackSql(SurveyModel surveyModel, Guid uniqueCode, DateTime now)
        {
            var providerAttributes = await ConvertSurveyToProviderAttributes(surveyModel);
            var feedbackId = await _employerEmailDetailRepository.GetFeedbackIdFromUniqueSurveyCode(uniqueCode);

            if (feedbackId == default(long))
            {
                throw new InvalidOperationException($"Unable to find feedback Id for Survey Invite {uniqueCode}");
            }

            try
            {
                var employerFeedbackResultId =
                    await _employerEmailDetailRepository.CreateEmployerFeedbackResult(
                    feedbackId,
                    surveyModel.Rating.Value.GetDisplayName(),
                    now,
                    providerAttributes);
                await _employerEmailDetailRepository.SetCodeBurntDate(uniqueCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to submit feedback");
            }
        }

        private async Task<IEnumerable<ProvideFeedback.Domain.Entities.Models.ProviderAttribute>> ConvertSurveyToProviderAttributes(SurveyModel surveyModel)
        {
            var feedbackQuestionAttributes = await _employerEmailDetailRepository.GetAllAttributes();
            var providerAttributes = new List<ProvideFeedback.Domain.Entities.Models.ProviderAttribute>();

            foreach (var attribute in surveyModel.Attributes.Where(s => s.Good || s.Bad))
            {
                var providerAttribute = feedbackQuestionAttributes.FirstOrDefault(s => s.AttributeName == attribute.Name);
                if (providerAttribute != null)
                {
                    providerAttributes.Add(new ProvideFeedback.Domain.Entities.Models.ProviderAttribute
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
