namespace ESFA.DAS.EmployerProvideFeedback.Infrastructure
{
    public class ProvideFeedbackEmployerWeb
    {
        public int SlidingExpirationMinutes { get; set; }
        public GoogleAnalyticsConfiguration GoogleAnalytics { get; set; }
        public ExternalLinksConfiguration ExternalLinks { get; set; }
        public string RedisConnectionString { get; set; }
        public string DataProtectionKeysDatabase { get; set; }
        public string EmployerFeedbackDatabaseConnectionString { get; set; }
        public int FeedbackWaitPeriodDays { get; set; }
        public string ZendeskSectionId { get; set; }
    }
}
