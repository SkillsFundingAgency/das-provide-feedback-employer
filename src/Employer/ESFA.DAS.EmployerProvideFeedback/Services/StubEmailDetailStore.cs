using System;

namespace ESFA.DAS.EmployerProvideFeedback.Services
{
    public class StubEmailDetailStore : IStoreEmailDetails
    {
        public bool IsFeedbackSubmittedFor(Guid uniqueCode)
        {
            return uniqueCode == new Guid("b84ffbbd-0ff7-4630-964d-e1c92400eed4");
        }
    }
}
