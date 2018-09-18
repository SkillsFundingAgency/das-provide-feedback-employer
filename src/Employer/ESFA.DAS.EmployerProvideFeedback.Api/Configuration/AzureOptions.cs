namespace ESFA.DAS.EmployerProvideFeedback.Configuration
{
    public class AzureOptions
    {
        public string AzureCosmosEndpoint { get; set; }

        public string AzureCosmosKey { get; set; }

        public string DatabaseName { get; set; }

        public string EmployerFeedbackCollection { get; set; }
    }
}