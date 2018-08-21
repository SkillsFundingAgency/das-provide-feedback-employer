using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Extensions;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.FeedbackDataAccess.Models;
using ESFA.DAS.FeedbackDataAccess.Repositories;

namespace ESFA.DAS.EmployerProvideFeedback.Orchestrators
{
    public class ReviewAnswersOrchestrator
    {
        private readonly IEmployerFeedbackRepository _employerFeedbackRepository;

        public ReviewAnswersOrchestrator(IEmployerFeedbackRepository employerFeedbackRepository)
        {
            _employerFeedbackRepository = employerFeedbackRepository;
        }

        public async Task SubmitConfirmedEmployerFeedback(AnswerModel answerModel)
        {
            var employerFeedback = ConvertToEmployerFeedback(answerModel);
            await _employerFeedbackRepository.CreateItemAsync(employerFeedback);
        }

        private EmployerFeedback ConvertToEmployerFeedback(AnswerModel answers)
        {
            return new EmployerFeedback
            {
                Id = Guid.NewGuid(),
                AccountId = 1,
                DateTimeCompleted = DateTime.Now,
                Ukprn = 11111111,
                ProviderAttributes = answers.ProviderSkills.Select(ps =>
                {
                    return new ProviderAttribute
                    {
                        Name = ps.Name,
                        Value = ps.Score
                    };

                }).ToList(),
                ProviderRating = answers.ProviderRating.Value.GetDisplayName()
            };
        }
    }
}
