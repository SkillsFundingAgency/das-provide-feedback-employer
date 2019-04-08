using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Http;

namespace ESFA.DAS.Feedback.Employer.Emailer.Configuration
{
    public class CommitmentsApiClientConfig : ICommitmentsApiClientConfiguration, IJwtClientConfiguration
    {
        public string BaseUrl { get; set; }
        public string ClientToken { get; set; }
    }
}
