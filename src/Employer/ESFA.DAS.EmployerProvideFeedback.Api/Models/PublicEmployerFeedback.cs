namespace ESFA.DAS.EmployerProvideFeedback.Api.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Feedback given by and Employer, omitting sensitive information such as Account Id and UserRef
    /// </summary>
    [Serializable]
    public class PublicEmployerFeedback
    {
        [JsonProperty(PropertyName = "ukprn")]
        public long Ukprn { get; set; }

        [JsonProperty(PropertyName = "dateTimeCompleted")]
        public DateTime DateTimeCompleted { get; set; }

        [JsonProperty(PropertyName = "providerAttributes")]
        public List<ProviderAttribute> ProviderAttributes { get; set; }

        [JsonProperty(PropertyName = "providerRating")]
        public string ProviderRating { get; set; }
    }
}
