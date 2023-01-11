using ESFA.DAS.EmployerProvideFeedback.Api.Models;
using MediatR;


namespace ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackResultQuery
{
    public class FeedbackResultQuery : IRequest<EmployerFeedbackResultDto>
    {
        public long Ukprn { get; set; }
    }
}
