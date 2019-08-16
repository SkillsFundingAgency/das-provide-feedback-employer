using System;

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Messages
{
    public class GenerateSurveyCodeMessage
    {
        public Guid UserRef { get; set; }
        public long Ukprn { get; set; }
        public long AccountId { get; set; }
    }
}
