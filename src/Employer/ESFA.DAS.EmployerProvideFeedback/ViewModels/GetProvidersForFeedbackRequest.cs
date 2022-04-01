using ESFA.DAS.EmployerProvideFeedback.Attributes;
using SFA.DAS.Encoding;

namespace ESFA.DAS.EmployerProvideFeedback.ViewModels
{
    public class GetProvidersForFeedbackRequest
    {
        public string EncodedAccountId { get; set; }

        [AutoDecode(nameof(EncodedAccountId), EncodingType.AccountId)]
        public long AccountId { get; set; }
    }
}
