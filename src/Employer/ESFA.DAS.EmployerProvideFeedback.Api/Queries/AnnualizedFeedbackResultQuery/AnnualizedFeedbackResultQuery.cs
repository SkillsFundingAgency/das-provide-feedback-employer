using System;
using MediatR;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;


namespace ESFA.DAS.EmployerProvideFeedback.Api.Queries.AnnualizedFeedbackResultQuery
{
    public class AnnualizedFeedbackResultQuery : IRequest<EmployerAnnualizedFeedbackResultDto>
    {
        public long Ukprn { get; set; }
        public String  AcademicYear { get; set; }
    }
}
