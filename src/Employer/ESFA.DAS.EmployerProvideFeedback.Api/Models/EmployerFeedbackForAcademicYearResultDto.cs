
using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace ESFA.DAS.EmployerProvideFeedback.Api.Models
{
    [Serializable]
    public class EmployerFeedbackForAcademicYearResultDto : EmployerFeedbackStarsSummaryDto
    {
        [JsonProperty(PropertyName = "providerAttribute")]
        public IEnumerable<ProviderAttributeForAcademicYearSummaryItemDto> ProviderAttribute { get; set; }
    }
}
