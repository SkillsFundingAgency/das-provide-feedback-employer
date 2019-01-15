using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Domain.Entities;

namespace ESFA.DAS.ProvideFeedback.Data
{
    public interface IStoreEmployerEmailDetails
    {
        Task<IEnumerable<EmployerEmailDetail>> GetEmailDetailsToBeSentInvite(int minDaysSincePreviousSurvey);
        Task SetEmailDetailsAsSent(Guid userRef);
        Task<EmployerEmailDetail> GetEmailDetailsForUniqueCode(Guid guid);
        Task SetCodeBurntDate(Guid uniqueCode);
        Task<bool> IsCodeBurnt(Guid emailCode);
        Task<IEnumerable<EmployerEmailDetail>> GetEmailDetailsToBeSentReminder(int minDaysSinceSent);
        Task SetEmailReminderAsSent(Guid userRef);
    }
}