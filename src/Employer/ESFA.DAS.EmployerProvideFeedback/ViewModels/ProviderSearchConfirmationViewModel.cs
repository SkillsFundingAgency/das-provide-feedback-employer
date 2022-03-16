namespace ESFA.DAS.EmployerProvideFeedback.ViewModels
{
    public class ProviderSearchConfirmationViewModel
    {
        //public long AccountId { get; set; }
        public string EncodedAccountId { get; set; }
        public long ProviderId { get; set; }
        public string ProviderName { get; set; }
        public bool? Confirmed { get; set; }
    }
}
