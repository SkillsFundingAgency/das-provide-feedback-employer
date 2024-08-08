using MediatR;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;

namespace ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackResultAnnualQuery
{
    public class FeedbackResultAnnualQuery : IRequest<EmployerFeedbackAnnualResultDto>
    {
        public long Ukprn { get; set; }
    }
}
