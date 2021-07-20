namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.Configuration
{
    public interface IAccountApiConfiguration
    {
        /// <summary>
        /// The base url (schema, server, port and application path as appropriate)
        /// </summary>
        /// <example>https://some-server/</example>
        string ApiBaseUrl { get; }

        /// <summary>
        /// The location of the resource that you are trying to access in Azure AD
        /// </summary>
        string IdentifierUri { get; }
    }
}
