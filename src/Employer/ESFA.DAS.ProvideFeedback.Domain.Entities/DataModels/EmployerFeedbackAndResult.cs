using System;

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Models
{
    public class EmployerFeedbackAndResult : EmployerFeedback
    {
        public DateTime? DateTimeCompleted { get; set; }
    }
}
