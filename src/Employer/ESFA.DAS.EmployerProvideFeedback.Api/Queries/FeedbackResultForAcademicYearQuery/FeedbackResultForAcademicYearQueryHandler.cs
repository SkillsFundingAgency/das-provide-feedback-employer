using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;

namespace ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackResultForAcademicYearQuery
{
    public class FeedbackResultForAcademicYearQueryHandler : IRequestHandler<FeedbackResultForAcademicYearQuery, EmployerFeedbackForAcademicYearResultDto>
    {
        private readonly IEmployerFeedbackRepository _employerfeedbackRepository;
        private readonly ILogger<FeedbackResultForAcademicYearQueryHandler> _logger;

        public FeedbackResultForAcademicYearQueryHandler(IEmployerFeedbackRepository employerFeedbackRepository, ILogger<FeedbackResultForAcademicYearQueryHandler> logger)
        {
            _employerfeedbackRepository = employerFeedbackRepository;
            _logger = logger;
        }
        public async Task<EmployerFeedbackForAcademicYearResultDto> Handle(FeedbackResultForAcademicYearQuery request, CancellationToken token)
        {
            IEnumerable<EmployerFeedbackResultSummary> feedback = await _employerfeedbackRepository.GetFeedbackResultSummaryForAcademicYear(request.Ukprn, request.AcademicYear);

            if (feedback == null || !feedback.Any())
            {
                return new EmployerFeedbackForAcademicYearResultDto()
                {
                    Ukprn = request.Ukprn,
                    ProviderAttribute = Enumerable.Empty<ProviderAttributeForAcademicYearSummaryItemDto>()
                };
            }

            IEnumerable<EmployerFeedbackForAcademicYearResultDto> grouped = feedback.GroupBy(
                x => new { x.Ukprn, x.Stars, x.ReviewCount,x.TimePeriod },
                x => new ProviderAttributeForAcademicYearSummaryItemDto
                {
                    Name = x.AttributeName,
                    Strength = x.Strength,
                    Weakness = x.Weakness,
                },
                (t, f) => new EmployerFeedbackForAcademicYearResultDto
                {
                    Ukprn = t.Ukprn,
                    Stars = t.Stars,
                    ReviewCount = t.ReviewCount,
                    TimePeriod = t.TimePeriod,
                    ProviderAttribute = f
                });

            return grouped.FirstOrDefault();
        }
    }
}
