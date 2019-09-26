using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Http;

namespace ESFA.DAS.ProvideFeedback.Employer.Application.Configuration
{
    public class CommitmentsApiClientConfig : ICommitmentsApiClientConfiguration, IJwtClientConfiguration
    {
        public string BaseUrl { get; set; }
        public string ClientToken { get; set; }
    }
}
