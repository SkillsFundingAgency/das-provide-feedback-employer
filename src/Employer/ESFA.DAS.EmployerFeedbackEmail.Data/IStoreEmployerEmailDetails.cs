using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Domain.Entities;

namespace ESFA.DAS.ProvideFeedback.Data
{
    public interface IStoreEmployerEmailDetails
    {
        Task<IEnumerable<EmployerSurveyInvite>> GetEmployerUsersToBeSentInvite(int minDaysSincePreviousSurvey);
        Task<EmployerSurveyInvite> GetEmployerInviteForUniqueCode(Guid guid);
        Task SetCodeBurntDate(Guid uniqueCode);
        Task<bool> IsCodeBurnt(Guid emailCode);
        Task<IEnumerable<EmployerSurveyInvite>> GetEmployerInvitesToBeSentReminder(int minDaysSinceSent);
        Task InsertSurveyInviteHistory(IEnumerable<Guid> uniqueSurveyCodes, int inviteType);
        Task<IEnumerable<EmployerSurveyInvite>> GetEmployerInvitesForNextCycleAsync(int inviteCycleDays);
        Task InsertNewSurveyInviteCodes(IEnumerable<EmployerSurveyInvite> newCodesRequired);
        Task UpsertIntoUsers(User user);
        Task ResetFeedback();
        Task UpsertIntoProvidersAsync(Provider provider);
        Task UpsertIntoFeedbackAsync(User user, Provider provider);
    }
}