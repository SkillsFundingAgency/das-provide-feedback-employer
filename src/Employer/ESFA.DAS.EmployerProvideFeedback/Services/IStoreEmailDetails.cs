using System;

namespace ESFA.DAS.EmployerProvideFeedback.Services
{
    public interface IStoreEmailDetails
    {
        bool IsFeedbackSubmittedFor(Guid uniqueCode);
    }
}
