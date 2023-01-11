namespace ESFA.DAS.Feedback.Employer.Emailer.Configuration
{
    public class EmailSettings
    {
        public string FeedbackSiteBaseUrl { get; set; }
        public int BatchSize { get; set; }
        public int ReminderDays { get; set; }
        public int InviteCycleDays { get; set; }
    }
}