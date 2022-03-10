using System;

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Models
{
    public class EmployerFeedback
    {
        public long FeedbackId { get; set; }
        public Guid UserRef { get; set; }
        public long Ukprn { get; set; }
        public long AccountId { get; set; }
        public bool IsActive { get; set; }
    }
}
