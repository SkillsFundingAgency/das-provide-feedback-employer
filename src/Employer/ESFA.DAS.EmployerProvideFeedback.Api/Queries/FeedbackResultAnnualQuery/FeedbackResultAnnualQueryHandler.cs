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
                    AnnualEmployerFeedbackDetails = Enumerable.Empty<EmployerFeedbackStarsAnnualSummaryDto>()
                };
            }

            var grouped = feedback
                .Where(f => f.AttributeName != null)  
                .GroupBy(
                    x => new { x.Ukprn, x.TimePeriod, x.Stars, x.ReviewCount },
                    (key, group) => new EmployerFeedbackStarsAnnualSummaryDto
                    {
                        Ukprn = key.Ukprn,
                        TimePeriod = key.TimePeriod,
                        Stars = key.Stars,
                        ReviewCount = key.ReviewCount,
                        ProviderAttribute = group
                            .Where(g => g.AttributeName != null)
                            .Select(g => new ProviderAttributeAnnualSummaryItemDto
                            {
                                Name = g.AttributeName,
                                Strength = g.Strength,
                                Weakness = g.Weakness
                            })
                            .Distinct()
                            .ToList()
                    })
                .ToList();

            return new EmployerFeedbackAnnualResultDto
            {
                AnnualEmployerFeedbackDetails = grouped
            };
        }
    }
}
