using System;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Messages
{
    [Serializable]
    public class EmployerFeedbackRefreshMessage
    {
        [JsonProperty("providerId")]
        public long ProviderId { get; set; }
        [JsonProperty("user")]
        public User User { get; set; }
    }
}
