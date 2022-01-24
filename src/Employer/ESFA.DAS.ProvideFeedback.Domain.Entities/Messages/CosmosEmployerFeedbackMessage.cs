using System;
using System.Collections.Generic;

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Messages
{
    public class CosmosEmployerFeedbackMessage
    {
        public string Id { get; set; }
        public Guid UserRef { get; set; }
        public long Ukprn { get; set; }
        public long AccountId { get; set; }
        public string ProviderRating { get; set; }
        public DateTime DateTimeCompleted { get; set; }
        public IEnumerable<FeedbackAnswer> FeedbackAnswers { get; set; }
    }

    public class FeedbackAnswer
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}
