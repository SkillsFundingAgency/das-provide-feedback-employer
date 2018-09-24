namespace ESFA.DAS.EmployerProvideFeedback.Api.Dto
{
    using System;

    using ESFA.DAS.EmployerProvideFeedback.Api.Repository;

    using Newtonsoft.Json;

    [Serializable]
    public class ApplicationKey : TypedDocument<ApplicationKey>
    {
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}