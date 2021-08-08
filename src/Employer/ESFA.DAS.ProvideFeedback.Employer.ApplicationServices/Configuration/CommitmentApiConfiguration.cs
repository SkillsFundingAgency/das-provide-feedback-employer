namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.Configuration
{

    public interface ICommitmentApiConfiguration
    {
        public string ApiBaseUrl { get; }

        public string IdentifierUri { get; }
    }

    public class CommitmentApiConfiguration : ICommitmentApiConfiguration
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
