using System;
using System.Collections.Generic;
using ESFA.DAS.EmployerProvideFeedback.Api.Repository;
using Newtonsoft.Json;

namespace ESFA.DAS.EmployerProvideFeedback.Api.Dto
{
    [Serializable]
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
    }

    [Serializable]
    public class ProviderAttribute
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public int Value { get; set; }
    }

    [Serializable]
    public class UserModel
    {
        public string Email { get; set; }

        public string Name { get; set; }
    }

    [Serializable]
    public class ApiAccount : TypedDocument<ApiAccount>
    {
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}