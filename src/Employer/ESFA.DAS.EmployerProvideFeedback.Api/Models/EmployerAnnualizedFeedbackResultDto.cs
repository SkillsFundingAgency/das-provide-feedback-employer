
using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace ESFA.DAS.EmployerProvideFeedback.Api.Models
{
    [Serializable]
    public class EmployerAnnualizedFeedbackResultDto : EmployerFeedbackStarsSummaryDto
    {
        [JsonProperty(PropertyName = "providerAttribute")]
        public IEnumerable<ProviderAttributeAnnualizedSummaryItemDto> ProviderAttribute { get; set; }
    }
}
