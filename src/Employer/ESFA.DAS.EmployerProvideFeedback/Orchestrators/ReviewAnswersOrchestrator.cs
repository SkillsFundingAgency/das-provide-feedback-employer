using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Extensions;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using ESFA.DAS.ProvideFeedback.Data.Repositories;

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

        public async Task SubmitConfirmedEmployerFeedback(SurveyModel surveyModel, Guid uniqueCode)
        {
            var providerAttributes = await ConvertSurveyToProviderAttributes(surveyModel);
            var feedbackId = await _employerFeedbackRepository.GetFeedbackIdFromUniqueSurveyCode(uniqueCode);

            if (feedbackId == default(long))
            {
                throw new InvalidOperationException($"Unable to find feedback Id for Survey Invite {uniqueCode}");
            }

            try
            {
                var employerFeedbackResultId =
                    await _employerFeedbackRepository.CreateEmployerFeedbackResult(
                    feedbackId,
                    surveyModel.Rating.Value.GetDisplayName(),
                    DateTime.UtcNow,
                    providerAttributes);
                await _employerFeedbackRepository.SetCodeBurntDate(uniqueCode);
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
