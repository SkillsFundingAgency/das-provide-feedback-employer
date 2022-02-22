namespace ESFA.DAS.EmployerProvideFeedback.Configuration.Routing
{
    public static class RoutePrefixPaths
    {
        public const string FeedbackRoutePath = "{uniqueCode:guid}";
        public const string FeedbackLandingPageRoutePath = "{encodedAccountId}/{uniqueCode:guid}";
    }
}
