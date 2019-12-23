using System;

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Models
{
    public class FeedbackInvite
    {
        public int FeedbackId { get; set; }
        public Guid? UniqueSurveyCode { get; set; }
        public DateTime? InviteSentDate { get; set; }
    }
}
