using System.Collections.Generic;

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Models
{
    public class FeedbackQuestionAttribute
    {
        public long AttributeId { get; set; }
        public string AttributeName { get; set; }
        public ICollection<ProviderAttribute> ProviderAttributes { get; set; }
    }
}
