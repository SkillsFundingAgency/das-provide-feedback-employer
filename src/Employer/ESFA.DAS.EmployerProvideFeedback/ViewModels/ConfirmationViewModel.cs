namespace ESFA.DAS.EmployerProvideFeedback.ViewModels
{
    public class ConfirmationViewModel
    {
        public string ProviderName { get; set; }

        public ProviderRating FeedbackRating { get; set; }

        public FeedbackViewModel Feedback { get; set; }

        public long Ukprn { get; set; }

        public string FatProviderDetailViewUrl { get; set; }

        public string FatProviderSearch { get; internal set; }
    }
}
