using System;

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Models
{
    public class ProviderAttribute
    {
        public Guid EmployerFeedbackResultId { get; set; }
        public long AttributeId { get; set; }
        public int AttributeValue { get; set; }
    }
}
