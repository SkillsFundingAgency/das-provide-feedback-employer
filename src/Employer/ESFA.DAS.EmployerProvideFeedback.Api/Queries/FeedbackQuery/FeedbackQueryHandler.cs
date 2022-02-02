using ESFA.DAS.EmployerProvideFeedback.Api.Models;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackQuery
{
    public class FeedbackQueryHandler : IRequestHandler<FeedbackQuery, IEnumerable<EmployerFeedbackDto>>
    {
        private readonly IEmployerFeedbackRepository _employerfeedbackRepository;
        private readonly ILogger<FeedbackQueryHandler> _logger;

        public FeedbackQueryHandler(IEmployerFeedbackRepository employerFeedbackRepository, ILogger<FeedbackQueryHandler> logger)
        {
            _employerfeedbackRepository = employerFeedbackRepository;
            _logger = logger;
        }
        public async Task<IEnumerable<EmployerFeedbackDto>> Handle(FeedbackQuery request, CancellationToken token)
        {
            var feedback = await _employerfeedbackRepository.GetEmployerFeedback();

            if (feedback == null || !feedback.Any())
            {
                return Enumerable.Empty<EmployerFeedbackDto>();
            }

            var groupedFeedback = feedback.GroupBy(
                x => new { x.Id, x.Ukprn, x.DateTimeCompleted, x.ProviderRating},
                x => new ProviderAttributeDto
                {
                    Name = x.AttributeName,
                    Value = x.AttributeValue
                },
                (t, f) => new EmployerFeedbackDto
                {
                    DateTimeCompleted = t.DateTimeCompleted,
                    ProviderRating = t.ProviderRating,
                    Ukprn = t.Ukprn,
                    ProviderAttributes = new List<ProviderAttributeDto>(f)
                });

            return groupedFeedback;
        }
    }
}
