using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DAS.Feedback.Employer.Emailer.Models
{
    public class DataRefreshMessage
    {
        public Provider Provider { get; set; }
        public User User { get; set; }
    }
}
