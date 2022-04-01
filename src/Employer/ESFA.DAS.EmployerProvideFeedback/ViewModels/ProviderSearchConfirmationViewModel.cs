namespace ESFA.DAS.EmployerProvideFeedback.ViewModels
{
    public class ProviderSearchConfirmationViewModel
    {
        public string EncodedAccountId { get; set; }
        public long ProviderId { get; set; }
        public string ProviderName { get; set; }
        public bool? Confirmed { get; set; }
    }
}
