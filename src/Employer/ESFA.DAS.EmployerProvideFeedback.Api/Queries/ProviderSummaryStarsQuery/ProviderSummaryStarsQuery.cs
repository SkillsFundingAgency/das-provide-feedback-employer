
using ESFA.DAS.EmployerProvideFeedback.Api.Models;
using MediatR;
using System.Collections.Generic;


namespace ESFA.DAS.EmployerProvideFeedback.Api.Queries.ProviderSummaryStarsQuery
{
    public class ProviderSummaryStarsQuery : IRequest<IEnumerable<EmployerFeedbackStarsSummaryDto>>
    {
    }
}
