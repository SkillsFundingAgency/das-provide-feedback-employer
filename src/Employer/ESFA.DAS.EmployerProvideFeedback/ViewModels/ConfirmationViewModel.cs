namespace ESFA.DAS.EmployerProvideFeedback.ViewModels
{
    public class ConfirmationViewModel
    {
        public string ProviderName { get; set; }

        public ProviderRating FeedbackRating { get; set; }

        public string FatUrl { get; internal set; }
    }
}
