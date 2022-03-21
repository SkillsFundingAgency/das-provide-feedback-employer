using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Models
{
    public class EmployerFeedbackViewModel
    {
        public Guid Id { get; set; }
        public long FeedbackId { get; set; }
        public long Ukprn { get; set; }
        public DateTime DateTimeCompleted { get; set; }
        public string ProviderRating { get; set; }
        public string AttributeName { get; set; }
        public int AttributeValue { get; set; }
    }
}
