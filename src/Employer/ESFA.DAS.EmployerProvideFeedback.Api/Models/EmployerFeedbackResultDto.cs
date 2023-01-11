
using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace ESFA.DAS.EmployerProvideFeedback.Api.Models
{
    [Serializable]
    public class EmployerFeedbackResultDto : EmployerFeedbackStarsSummaryDto
    {
        [JsonProperty(PropertyName = "providerAttribute")]
        public IEnumerable<ProviderAttributeSummaryItemDto> ProviderAttribute { get; set; }
    }
}
