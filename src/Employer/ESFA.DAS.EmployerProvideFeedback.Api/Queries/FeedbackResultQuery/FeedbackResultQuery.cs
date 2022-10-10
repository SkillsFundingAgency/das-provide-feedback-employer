
using System.Collections.Generic;
using MediatR;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;


namespace ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackResultQuery
{
    public class FeedbackResultQuery : IRequest<EmployerFeedbackResultDto>
    {
        public long Ukprn { get; set; }
    }
}
