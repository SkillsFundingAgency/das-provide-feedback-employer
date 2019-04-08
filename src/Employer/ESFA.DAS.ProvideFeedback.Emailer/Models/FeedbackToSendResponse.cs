using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DAS.Feedback.Employer.Emailer.Models
{
    public class FeedbackToSendResponse
    {
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string ProviderName { get; set; }
        public long AccountId { get; set; }
        public long Ukprn { get; set; }
        public Guid UserRef { get; set; }
    }
}
