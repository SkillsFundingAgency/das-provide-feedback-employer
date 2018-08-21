using Newtonsoft.Json;

namespace ESFA.DAS.FeedbackDataAccess.Models
{
    public class ProviderAttribute
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public int Value { get; set; }
    }
}
