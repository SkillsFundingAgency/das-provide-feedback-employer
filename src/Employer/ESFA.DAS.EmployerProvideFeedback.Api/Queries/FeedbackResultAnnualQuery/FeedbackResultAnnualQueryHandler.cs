using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;

namespace ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackResultAnnualQuery
{
    public class FeedbackResultAnnualQueryHandler : IRequestHandler<FeedbackResultAnnualQuery, EmployerFeedbackAnnualResultDto>
    {
        private readonly IEmployerFeedbackRepository _employerfeedbackRepository;
        private readonly ILogger<FeedbackResultAnnualQueryHandler> _logger;

        public FeedbackResultAnnualQueryHandler(IEmployerFeedbackRepository employerFeedbackRepository, ILogger<FeedbackResultAnnualQueryHandler> logger)
        {
            _employerfeedbackRepository = employerFeedbackRepository;
            _logger = logger;
        }
        public async Task<EmployerFeedbackAnnualResultDto> Handle(FeedbackResultAnnualQuery request, CancellationToken token)
        {
            IEnumerable<EmployerFeedbackResultSummary> feedback = await _employerfeedbackRepository.GetFeedbackResultSummaryAnnual(request.Ukprn);

            if (feedback == null || !feedback.Any())
            {
                return new EmployerFeedbackAnnualResultDto()
                {
                    Ukprn = request.Ukprn,
                    ProviderAttribute = Enumerable.Empty<ProviderAttributeAnnualSummaryItemDto>()
                };
            }

            IEnumerable<EmployerFeedbackAnnualResultDto> grouped = feedback.GroupBy(
                x => new { x.Ukprn, x.Stars, x.ReviewCount },
                x => new ProviderAttributeAnnualSummaryItemDto
                {
                    Name = x.AttributeName,
                    Strength = x.Strength,
                    Weakness = x.Weakness,
                    TimePeriod = x.TimePeriod
                },
                (t, f) => new EmployerFeedbackAnnualResultDto
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
