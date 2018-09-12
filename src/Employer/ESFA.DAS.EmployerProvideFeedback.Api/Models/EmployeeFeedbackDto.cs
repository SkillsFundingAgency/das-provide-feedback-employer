using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ESFA.DAS.FeedbackDataAccess.Models;
using Newtonsoft.Json;

namespace ESFA.DAS.EmployerProvideFeedback.Api.Models
{
    public class EmployerFeedbackDto
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "ukprn")]
        public long Ukprn { get; set; }

        [JsonProperty(PropertyName = "accountId")]
        public long AccountId { get; set; }

        [JsonProperty(PropertyName = "userRef")]
        public Guid UserRef { get; set; }

        [JsonProperty(PropertyName = "dateTimeCompleted")]
        public DateTime DateTimeCompleted { get; set; }

        [JsonProperty(PropertyName = "providerAttributes")]
        public List<ProviderAttribute> ProviderAttributes { get; set; }

        [JsonProperty(PropertyName = "providerRating")]
        public string ProviderRating { get; set; }
    }
}