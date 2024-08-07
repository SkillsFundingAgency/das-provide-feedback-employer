﻿
using System;
using Newtonsoft.Json;


namespace ESFA.DAS.EmployerProvideFeedback.Api.Models
{
    /// <summary>
    /// Strength and Weakness counts of a Provider
    /// </summary>
    [Serializable]
    public class ProviderAttributeForAcademicYearSummaryItemDto
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "strength")]
        public int Strength{ get; set; }

        [JsonProperty(PropertyName = "weakness")]
        public int Weakness { get; set; }
    }
}
