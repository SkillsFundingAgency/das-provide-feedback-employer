namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.Configuration
{
    public class AccountApiConfiguration : IAccountApiConfiguration
    {
        /// <summary>
        /// The base url (schema, server, port and application path as appropriate)
        /// </summary>
        /// <example>https://some-server/</example>
        public string ApiBaseUrl { get; set; }

        /// <summary>
        /// The location of the resource that you are trying to access in Azure AD
        /// </summary>
        public string IdentifierUri { get; set; }
    }
}
