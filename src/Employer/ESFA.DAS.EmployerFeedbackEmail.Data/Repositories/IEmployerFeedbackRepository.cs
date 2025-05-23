
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Data.Enums;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;


namespace ESFA.DAS.ProvideFeedback.Data.Repositories
{
    public interface IEmployerFeedbackRepository
    {
        Task<EmployerSurveyInvite> GetEmployerInviteForUniqueCode(Guid guid);
        Task SetCodeBurntDate(Guid uniqueCode);
        Task<DateTime?> GetCodeBurntDate(Guid uniqueCode);
        Task<bool> IsCodeBurnt(Guid emailCode);
        Task MarkProviderInactive();
        Task InsertSurveyInviteHistory(IEnumerable<Guid> uniqueSurveyCodes, int inviteType);
        Task InsertNewSurveyForFeedback(long feedbackId);
        Task UpsertIntoUsers(User user);
        Task ResetFeedback();
        Task UpsertIntoProviders(IEnumerable<Provider> providers);
        Task<long> UpsertIntoFeedback(Guid userRef, long accountId, long ukprn);
        Task<EmployerSurveyInvite> GetEmployerSurveyInvite(long feedbackId);
        Task<IEnumerable<Provider>> GetProvidersByUkprn(IEnumerable<long> commitmentUkprns);
        Task<Provider> GetProviderByUkprn(long ukprn);
        Task<User> GetUserByUserRef(Guid userRef);
        Task<EmployerFeedback> GetEmployerFeedbackRecord(Guid userRef, long accountId, long ukprn);
        Task<EmployerFeedbackResult> GetEmployerFeedbackResultRecord(long feedbackId, DateTime datetimeCompleted);
        Task<IEnumerable<FeedbackQuestionAttribute>> GetAllAttributes();
        Task<Guid> CreateEmployerFeedbackResult(long feedbackId, string providerRating, DateTime dateTimeCompleted, FeedbackSource feedbackSource, IEnumerable<ProviderAttribute> providerAttributes);
        Task<long> GetFeedbackIdFromUniqueSurveyCode(Guid uniqueCode);
        Task<Guid> GetUniqueSurveyCodeFromFeedbackId(long feedbackId);
        Task<IEnumerable<EmployerFeedbackViewModel>> GetEmployerFeedback();
        Task<IEnumerable<EmployerFeedbackAndResult>> GetAllFeedbackAndResultFromEmployer(long accountId);
        Task<int> GenerateProviderRatingResults(int allUserFeedback, int resultsforAllTime, int recentFeedbackMonths, decimal tolerance);
        Task<int> GenerateProviderAttributeResults(int allUserFeedback, int resultsforAllTime, int recentFeedbackMonths);
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummary(long ukprn);
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummaryAnnual(long ukprn);
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummaryForAcademicYear(long ukprn, String AcademicYear);
        Task<IEnumerable<ProviderStarsSummary>> GetAllStarsSummary();
    }
}
