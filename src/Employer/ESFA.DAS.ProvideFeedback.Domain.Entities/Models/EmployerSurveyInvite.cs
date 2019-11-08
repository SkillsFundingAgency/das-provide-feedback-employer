using System;

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Models
{
    public class EmployerSurveyInvite
    {
        public Guid UniqueSurveyCode { get; set; }
        public Guid UserRef { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public long AccountId { get; set; }
        public long Ukprn { get; set; }
        public string ProviderName { get; set; }
        public DateTime? InviteSentDate { get; set; }
        public DateTime? LastReminderSentDate { get; set; }
        public DateTime? CodeBurntDate { get; set; }
    }
}
