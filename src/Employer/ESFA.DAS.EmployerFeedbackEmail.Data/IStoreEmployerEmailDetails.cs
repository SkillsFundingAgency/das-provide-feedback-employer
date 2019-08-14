using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;

namespace ESFA.DAS.ProvideFeedback.Data
{
    public interface IStoreEmployerEmailDetails
    {
        Task<IEnumerable<EmployerSurveyInvite>> GetEmployerUsersToBeSentInvite();
        Task<EmployerSurveyInvite> GetEmployerInviteForUniqueCode(Guid guid);
        Task SetCodeBurntDate(Guid uniqueCode);
        Task<bool> IsCodeBurnt(Guid emailCode);
        Task<IEnumerable<EmployerSurveyInvite>> GetEmployerInvitesToBeSentReminder(int minDaysSinceSent);
        Task InsertSurveyInviteHistory(IEnumerable<Guid> uniqueSurveyCodes, int inviteType);
        Task InsertNewSurveyForFeedback(long feedbackId);
        Task UpsertIntoUsers(User user);
        Task ResetFeedback();
        Task UpsertIntoProviders(Provider provider);
        Task<long> UpsertIntoFeedback(Guid userRef, long accountId, long ukprn);
        Task<bool> GetNewCodeRequired(long feedbackId, int minDaysSinceInvite);
        Task<EmployerSurveyInvite> GetEmployerSurveyInvite(long feedbackId);
    }
}