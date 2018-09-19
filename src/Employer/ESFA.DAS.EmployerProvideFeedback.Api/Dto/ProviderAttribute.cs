namespace ESFA.DAS.EmployerProvideFeedback.Api.Dto
{
    using System;

    using Newtonsoft.Json;

    [Serializable]
    public class ProviderAttribute
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public int Value { get; set; }
    }
}