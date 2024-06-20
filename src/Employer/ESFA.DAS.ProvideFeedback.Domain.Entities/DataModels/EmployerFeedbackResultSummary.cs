
using System;


namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Models
{
    public class EmployerFeedbackResultSummary : ProviderStarsSummary
    {
        public string AttributeName { get; set; }
        public int Strength { get; set; }
        public int Weakness { get; set; }
        public string TimePeriod { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
