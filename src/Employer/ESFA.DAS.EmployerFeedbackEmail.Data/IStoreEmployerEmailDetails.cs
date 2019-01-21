using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Domain.Entities;

namespace ESFA.DAS.ProvideFeedback.Data
{
    public interface IStoreEmployerEmailDetails
    {
        Task<IEnumerable<EmployerSurveyInvite>> GetEmailDetailsToBeSentInvite(int minDaysSincePreviousSurvey);
        // Task SetEmailDetailsAsSent(Guid userRef);
        Task<EmployerSurveyInvite> GetEmailDetailsForUniqueCode(Guid guid);
        Task SetCodeBurntDate(Guid uniqueCode);
        Task<bool> IsCodeBurnt(Guid emailCode);
        Task<IEnumerable<EmployerSurveyInvite>> GetEmailDetailsToBeSentReminder(int minDaysSinceSent);
        Task InsertSurveyInviteHistory(IEnumerable<Guid> uniqueSurveyCodes, int inviteType);
    }
}