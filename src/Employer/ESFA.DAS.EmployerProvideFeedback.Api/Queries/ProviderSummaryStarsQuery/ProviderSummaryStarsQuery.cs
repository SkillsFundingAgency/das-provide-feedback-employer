
using System.Collections.Generic;
using MediatR;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;


namespace ESFA.DAS.EmployerProvideFeedback.Api.Queries.ProviderSummaryStarsQuery
{
    public class ProviderSummaryStarsQuery : IRequest<IEnumerable<EmployerFeedbackForStarsSummaryDto>>
    {
        public string TimePeriod { get; set; }
    }
}
