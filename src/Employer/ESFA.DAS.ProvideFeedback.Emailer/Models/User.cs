using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DAS.Feedback.Employer.Emailer.Models
{
    public class User
    {
        public Guid UserRef { get; set; }
        public long AccountId { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
    }
}
