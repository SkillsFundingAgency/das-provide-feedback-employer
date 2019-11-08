using System;

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Models
{
    public class User
    {
        public Guid UserRef { get; set; }
        public long AccountId { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
    }
}
