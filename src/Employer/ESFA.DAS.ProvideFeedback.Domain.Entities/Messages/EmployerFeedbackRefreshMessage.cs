using ESFA.DAS.ProvideFeedback.Domain.Entities;
using Newtonsoft.Json;
using System;

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Messages
{
    [Serializable]
    public class EmployerFeedbackRefreshMessage
    {
        [JsonProperty("provider")]
        public Provider Provider { get; set; }
        [JsonProperty("user")]
        public User User { get; set; }
    }
}
