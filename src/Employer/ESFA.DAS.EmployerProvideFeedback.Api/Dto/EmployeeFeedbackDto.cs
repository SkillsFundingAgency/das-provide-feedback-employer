using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ESFA.DAS.FeedbackDataAccess.Models;
using Newtonsoft.Json;

namespace ESFA.DAS.EmployerProvideFeedback.Api.Models
{
    using ESFA.DAS.EmployerProvideFeedback.Api.Repository;

    [Serializable]
    public class EmployerFeedbackDto : TypedDocument<EmployerFeedbackDto>
    {
        [JsonProperty(PropertyName = "ukprn")]
        public long Ukprn { get; set; }

        [JsonProperty(PropertyName = "accountId")]
        public long AccountId { get; set; }

        [JsonProperty(PropertyName = "userRef")]
        public Guid UserRef { get; set; }

        [JsonProperty(PropertyName = "dateTimeCompleted")]
        public DateTime DateTimeCompleted { get; set; }

        [JsonProperty(PropertyName = "providerAttributes")]
        public List<ProviderAttributeDto> ProviderAttributes { get; set; }

        [JsonProperty(PropertyName = "providerRating")]
        public string ProviderRating { get; set; }
    }

    [Serializable]
    public class ProviderAttributeDto
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public int Value { get; set; }
    }
}