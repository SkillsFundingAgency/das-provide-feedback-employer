using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Http.Configuration;

namespace ESFA.DAS.ProvideFeedback.Employer.Application.Configuration
{
    public class CommitmentsApiClientConfig : ICommitmentsApiClientConfiguration, IJwtClientConfiguration
    {
        public string BaseUrl { get; set; }
        public string ClientToken { get; set; }

        public string ApiBaseUrl => BaseUrl;

        public string Tenant => ""; // throw new System.NotImplementedException();

        public string ClientId => ""; // throw new System.NotImplementedException();

        public string ClientSecret => ""; // throw new System.NotImplementedException();

        public string IdentifierUri => ""; // throw new System.NotImplementedException();
    }
}
