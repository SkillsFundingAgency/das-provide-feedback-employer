using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DAS.Feedback.Employer.Emailer.Models
{
    [Serializable]
    public class DataRefreshMessage
    {
        [JsonProperty("provider")]
        public Provider Provider { get; set; }
        [JsonProperty("user")]
        public User User { get; set; }
    }
}
