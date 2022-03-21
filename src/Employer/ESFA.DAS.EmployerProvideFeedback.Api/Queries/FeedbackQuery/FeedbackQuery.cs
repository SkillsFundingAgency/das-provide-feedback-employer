using ESFA.DAS.EmployerProvideFeedback.Api.Models;
using MediatR;
using System.Collections.Generic;

namespace ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackQuery
{
    public class FeedbackQuery : IRequest<IEnumerable<EmployerFeedbackDto>>
    {
    }
}
