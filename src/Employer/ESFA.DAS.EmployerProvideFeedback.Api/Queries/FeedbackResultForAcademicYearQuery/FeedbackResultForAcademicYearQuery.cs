using MediatR;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;

namespace ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackResultForAcademicYearQuery
{
    public class FeedbackResultForAcademicYearQuery : IRequest<EmployerFeedbackForAcademicYearResultDto>
    {
        public long Ukprn { get; set; }
        public string  AcademicYear { get; set; }
    }
}
