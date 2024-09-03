
using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace ESFA.DAS.EmployerProvideFeedback.Api.Models
{
    [Serializable]
    public class EmployerFeedbackAnnualResultDto 
    {
        public IEnumerable<EmployerFeedbackStarsAnnualSummaryDto> AnnualEmployerFeedbackDetails { get; set; }
   }
}
