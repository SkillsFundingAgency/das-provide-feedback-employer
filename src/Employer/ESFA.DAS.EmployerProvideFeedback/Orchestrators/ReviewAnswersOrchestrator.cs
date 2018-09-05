using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Extensions;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.FeedbackDataAccess.Models;
using ESFA.DAS.FeedbackDataAccess.Repositories;
using ESFA.DAS.ProvideFeedback.Data;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using ProviderAttribute = ESFA.DAS.FeedbackDataAccess.Models.ProviderAttribute;

namespace ESFA.DAS.EmployerProvideFeedback.Orchestrators
{
    public class ReviewAnswersOrchestrator
    {
        private readonly IEmployerFeedbackRepository _employerFeedbackRepository;
        private readonly IStoreEmployerEmailDetails _employerEmailDetailRepository;
        private readonly ILogger<ReviewAnswersOrchestrator> _logger;

        public ReviewAnswersOrchestrator(IEmployerFeedbackRepository employerFeedbackRepository, IStoreEmployerEmailDetails employerEmailDetailRepository, ILogger<ReviewAnswersOrchestrator> logger)
        {
            _employerFeedbackRepository = employerFeedbackRepository;
            _employerEmailDetailRepository = employerEmailDetailRepository;
            _logger = logger;
        }

        public async Task SubmitConfirmedEmployerFeedback(SurveyModel surveyModel, Guid uniqueCode)
        {
            var employerFeedback = ConvertToEmployerFeedback(surveyModel);
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

        private EmployerFeedback ConvertToEmployerFeedback(SurveyModel survey)
        {
            return new EmployerFeedback
            {
                Id = Guid.NewGuid(),
                UserRef = survey.UserRef,
                AccountId = survey.AccountId,
                DateTimeCompleted = DateTime.Now,
                Ukprn = survey.Ukprn,
                ProviderAttributes = survey.ProviderAttributes.Select(ps =>
                {
                    return new ProviderAttribute
                    {
                        Name = ps.Name,
                        Value = ps.Score
                    };

                }).ToList(),
                ProviderRating = survey.ProviderRating.Value.GetDisplayName()
            };
        }
    }
}
