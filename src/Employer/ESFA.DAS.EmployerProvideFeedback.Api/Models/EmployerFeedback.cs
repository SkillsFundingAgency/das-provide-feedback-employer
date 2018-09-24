namespace ESFA.DAS.EmployerProvideFeedback.Api.Models
{
    using System;

    using Newtonsoft.Json;

    [Serializable]
    public class EmployerFeedback : PublicEmployerFeedback
    {
        [JsonProperty(PropertyName = "accountId")]
        public long AccountId { get; set; }

        [JsonProperty(PropertyName = "userRef")]
        public Guid UserRef { get; set; }
    }
}