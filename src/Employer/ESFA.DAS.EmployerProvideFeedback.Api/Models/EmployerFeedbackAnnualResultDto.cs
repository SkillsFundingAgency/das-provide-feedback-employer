
using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace ESFA.DAS.EmployerProvideFeedback.Api.Models
{
    [Serializable]
    public class EmployerFeedbackAnnualResultDto : EmployerFeedbackStarsSummaryDto
    {
        [JsonProperty(PropertyName = "providerAttribute")]
        public IEnumerable<ProviderAttributeAnnualSummaryItemDto> ProviderAttribute { get; set; }
    }
}
