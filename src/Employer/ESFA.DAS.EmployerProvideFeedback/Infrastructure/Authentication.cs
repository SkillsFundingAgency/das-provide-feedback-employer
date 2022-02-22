namespace ESFA.DAS.EmployerProvideFeedback.Infrastructure
{
    public class Authentication
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string BaseAddress { get; set; }
        public bool UsePkce { get; set; }
        public string Scopes { get; set; }
        public string ResponseType { get; set; } = "code";
    }
}
