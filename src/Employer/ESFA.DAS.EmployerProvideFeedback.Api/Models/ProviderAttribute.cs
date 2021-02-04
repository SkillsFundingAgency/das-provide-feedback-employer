using System;

namespace ESFA.DAS.EmployerProvideFeedback.Api.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// A positive or negative attribute of a Provider
    /// </summary>
    [Serializable]
    public class ProviderAttribute
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public int Value { get; set; }
    }
}
