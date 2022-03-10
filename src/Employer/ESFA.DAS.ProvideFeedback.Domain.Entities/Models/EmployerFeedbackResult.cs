using System;
using System.Collections.Generic;

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Models
{
    public class EmployerFeedbackResult
    {
        public Guid Id { get; set; }
        public long FeedbackId { get; set; }
        public DateTime DateTimeCompleted { get; set; }
        public string ProviderRating { get; set; }
        public IEnumerable<ProviderAttribute> ProviderAttributes { get; set; }
    }
}
