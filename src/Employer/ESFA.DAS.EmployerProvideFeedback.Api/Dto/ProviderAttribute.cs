namespace ESFA.DAS.EmployerProvideFeedback.Api.Dto
{
    using System;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    [Serializable]
    [DataContract(Name = "ProviderAttribute")]
    public class ProviderAttribute
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public int Value { get; set; }
    }
}