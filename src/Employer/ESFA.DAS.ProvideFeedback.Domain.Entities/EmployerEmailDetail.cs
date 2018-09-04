using System;

namespace Esfa.Das.ProvideFeedback.Domain.Entities
{
    public class EmployerEmailDetail
    {
        public Guid UserRef { get; set; }
        public string EmailAddress { get; set; }
        public string ProviderName { get; set; }
        public string UserFirstName { get; set; }
        public Guid EmailCode { get; set; }
    }
}