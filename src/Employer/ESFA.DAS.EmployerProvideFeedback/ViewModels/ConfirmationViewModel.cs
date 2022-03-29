namespace ESFA.DAS.EmployerProvideFeedback.ViewModels
{
    public class ConfirmationViewModel
    {
        public string ProviderName { get; set; }

        public ProviderRating FeedbackRating { get; set; }

        public string FatUrl { get; internal set; }
        public string ComplaintToProviderSiteUrl { get; set; }
        public string ComplaintSiteUrl { get; set; }
        public string EmployerAccountsHomeUrl { get; set; }
        public bool HasMultipleProviders { get; set; }
        public string EncodedAccountId { get; set; }
    }
}
