using ESFA.DAS.EmployerProvideFeedback.Attributes;
using SFA.DAS.Encoding;
using System;

namespace ESFA.DAS.EmployerProvideFeedback.ViewModels
{
    public class StartFeedbackRequest
    {
        public string EncodedAccountId { get; set; }

        [AutoDecode(nameof(EncodedAccountId), EncodingType.AccountId)]
        public long AccountId { get; set; }
        public Guid UniqueCode { get; set; }
    }
}
