
using Newtonsoft.Json;
using System;


namespace ESFA.DAS.EmployerProvideFeedback.Api.Models
{
    [Serializable]
    public class EmployerFeedbackStarsSummaryDto
    {
        [JsonProperty(PropertyName = "ukprn", Order = int.MinValue)]
        public long Ukprn { get; set; }

        [JsonProperty(PropertyName = "stars", Order = int.MinValue)]
        public int Stars { get; set; }

        [JsonProperty(PropertyName = "reviewCount", Order = int.MinValue)]
        public int ReviewCount { get; set; }
    }
}
