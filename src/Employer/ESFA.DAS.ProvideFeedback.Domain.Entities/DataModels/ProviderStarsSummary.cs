

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Models
{
    public class ProviderStarsSummary
    {
        public long Ukprn { get; set; }
        public int Stars { get; set; }
        public int ReviewCount { get; set; }
        public string TimePeriod { get; set; }
    }
}
