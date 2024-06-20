
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;

namespace ESFA.DAS.EmployerProvideFeedback.Api.Queries.AnnualizedFeedbackResultQuery
{
    public class AnnualizedFeedbackResultQueryHandler : IRequestHandler<AnnualizedFeedbackResultQuery, EmployerAnnualizedFeedbackResultDto>
    {
        private readonly IEmployerFeedbackRepository _employerfeedbackRepository;
        private readonly ILogger<AnnualizedFeedbackResultQueryHandler> _logger;

        public AnnualizedFeedbackResultQueryHandler(IEmployerFeedbackRepository employerFeedbackRepository, ILogger<AnnualizedFeedbackResultQueryHandler> logger)
        {
            _employerfeedbackRepository = employerFeedbackRepository;
            _logger = logger;
        }
        public async Task<EmployerAnnualizedFeedbackResultDto> Handle(AnnualizedFeedbackResultQuery request, CancellationToken token)
        {
            IEnumerable<EmployerFeedbackResultSummary> feedback = await _employerfeedbackRepository.GetAnnualizedFeedbackResultSummary(request.Ukprn, request.AcademicYear);

            if (feedback == null || !feedback.Any())
            {
                return new EmployerAnnualizedFeedbackResultDto()
                {
                    Ukprn = request.Ukprn,
                    ProviderAttribute = Enumerable.Empty<ProviderAttributeAnnualizedSummaryItemDto>()
                };
            }

            IEnumerable<EmployerAnnualizedFeedbackResultDto> grouped = feedback.GroupBy(
                x => new { x.Ukprn, x.Stars, x.ReviewCount },
                x => new ProviderAttributeAnnualizedSummaryItemDto
                {
                    Name = x.AttributeName,
                    Strength = x.Strength,
                    Weakness = x.Weakness,
                    TimePeriod = x.TimePeriod
                },
                (t, f) => new EmployerAnnualizedFeedbackResultDto
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
