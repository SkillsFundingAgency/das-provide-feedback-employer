using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;


namespace ESFA.DAS.EmployerProvideFeedback.Api.Queries.ProviderSummaryStarsQuery
{
    public class ProviderSummaryStarsQueryHandler : IRequestHandler<ProviderSummaryStarsQuery, IEnumerable<EmployerFeedbackStarsSummaryByPeriod>>
    {
        private readonly IEmployerFeedbackRepository _employerfeedbackRepository;
        private readonly ILogger<ProviderSummaryStarsQueryHandler> _logger;

        public ProviderSummaryStarsQueryHandler(IEmployerFeedbackRepository employerFeedbackRepository, ILogger<ProviderSummaryStarsQueryHandler> logger)
        {
            _employerfeedbackRepository = employerFeedbackRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<EmployerFeedbackStarsSummaryByPeriod>> Handle(ProviderSummaryStarsQuery request, CancellationToken token)
        {
            IEnumerable<ProviderStarsSummary> stars = await _employerfeedbackRepository.GetAllStarsSummary(request.TimePeriod);

            return stars?.Select(x => new EmployerFeedbackStarsSummaryByPeriod()
            {
                Ukprn = x.Ukprn,
                ReviewCount = x.ReviewCount,
                Stars = x.Stars,
            });
        }
    }
}
