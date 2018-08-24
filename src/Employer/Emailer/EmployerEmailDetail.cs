using System;

namespace Esfa.Das.Feedback.Employer.Emailer
{
    public class EmployerEmailDetail
    {
        public Guid UserRef { get; set; }
        public string Email { get; set; }
        public string ProviderName { get; internal set; }
        public string UserFirstName { get; internal set; }
        public Guid EmailCode { get; internal set; }
    }
}