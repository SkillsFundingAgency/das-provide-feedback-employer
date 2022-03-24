using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.Configuration;

namespace ESFA.DAS.EmployerProvideFeedback.Infrastructure
{
    public class ProvideFeedbackEmployerWebConfiguration
    {
        public int SlidingExpirationMinutes { get; set; }
        public int FeedbackWaitPeriodDays { get; set; }
        public string ZendeskSectionId { get; set; }
        public GoogleAnalyticsConfiguration GoogleAnalytics { get; set; }
        public ExternalLinksConfiguration ExternalLinks { get; set; }
        public string RedisConnectionString { get; set; }
        public string DataProtectionKeysDatabase { get; set; }
        public string EmployerFeedbackDatabaseConnectionString { get; set; }
        public Authentication Authentication { get; set; }
        public CommitmentApiConfiguration CommitmentsApiConfiguration { get; set; }
    }
}
