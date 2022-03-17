using System;

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Models
{
    public class EmployerFeedbackAndSurveyCode : EmployerFeedback
    {
        public Guid UniqueSurveyCode { get; set; }
        public DateTime? BurnDate { get; set; }
    }
}
