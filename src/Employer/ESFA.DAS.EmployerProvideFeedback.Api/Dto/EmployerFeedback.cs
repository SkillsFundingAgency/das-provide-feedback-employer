namespace ESFA.DAS.EmployerProvideFeedback.Api.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using ESFA.DAS.EmployerProvideFeedback.Api.Configuration.Mappers;
    using ESFA.DAS.EmployerProvideFeedback.Api.Models;
    using ESFA.DAS.EmployerProvideFeedback.Api.Repository;

    using Newtonsoft.Json;

    [Serializable]
    [DataContract(Name = "EmployerFeedback")]
    public class EmployerFeedback : TypedDocument<EmployerFeedback>
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
        public List<ProviderAttribute> ProviderAttributes { get; set; }

        [JsonProperty(PropertyName = "providerRating")]
        public string ProviderRating { get; set; }

        [JsonProperty(PropertyName = "_pk")]
        public new string PartitionKey => this.Ukprn.ToString();
    }
}