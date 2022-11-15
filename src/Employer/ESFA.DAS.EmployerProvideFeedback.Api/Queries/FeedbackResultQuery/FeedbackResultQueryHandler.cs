
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;


namespace ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackResultQuery
{
    public class FeedbackResultQueryHandler : IRequestHandler<FeedbackResultQuery, EmployerFeedbackResultDto>
    {
        private readonly IEmployerFeedbackRepository _employerfeedbackRepository;
        private readonly ILogger<FeedbackResultQueryHandler> _logger;

        public FeedbackResultQueryHandler(IEmployerFeedbackRepository employerFeedbackRepository, ILogger<FeedbackResultQueryHandler> logger)
        {
            _employerfeedbackRepository = employerFeedbackRepository;
            _logger = logger;
        }
        public async Task<EmployerFeedbackResultDto> Handle(FeedbackResultQuery request, CancellationToken token)
        {
            IEnumerable<EmployerFeedbackResultSummary> feedback = await _employerfeedbackRepository.GetFeedbackResultSummary(request.Ukprn);

            if (feedback == null || !feedback.Any())
            {
                return new EmployerFeedbackResultDto()
                {
                    Ukprn = request.Ukprn,
                    ProviderAttribute = Enumerable.Empty<ProviderAttributeSummaryItemDto>()
                };
            }

            IEnumerable<EmployerFeedbackResultDto> grouped = feedback.GroupBy(
                x => new { x.Ukprn, x.Stars, x.ReviewCount },
                x => new ProviderAttributeSummaryItemDto
                {
                    Name = x.AttributeName,
                    Strength = x.Strength,
                    Weakness = x.Weakness
                },
                (t, f) => new EmployerFeedbackResultDto
                {
                    Ukprn = t.Ukprn,
                    Stars = t.Stars,
                    ReviewCount = t.ReviewCount,
                    ProviderAttribute = f
                });

            return grouped.FirstOrDefault();
        }
    }
}
