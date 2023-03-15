using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Data.Enums;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;


namespace ESFA.DAS.ProvideFeedback.Data.Repositories
{
    public interface IEmployerFeedbackRepository
    {
        Task SetCodeBurntDate(Guid uniqueCode);
        Task<DateTime?> GetCodeBurntDate(Guid uniqueCode);
        Task<bool> IsCodeBurnt(Guid emailCode);
        Task UpsertIntoUsers(User user);
        Task UpsertIntoProviders(IEnumerable<Provider> providers);
        Task<long> UpsertIntoFeedback(Guid userRef, long accountId, long ukprn);
        Task<EmployerFeedback> GetEmployerFeedbackRecord(Guid userRef, long accountId, long ukprn);
        Task<IEnumerable<FeedbackQuestionAttribute>> GetAllAttributes();
        Task<Guid> CreateEmployerFeedbackResult(long feedbackId, string providerRating, DateTime dateTimeCompleted, FeedbackSource feedbackSource, IEnumerable<ProviderAttribute> providerAttributes);
        Task<Guid> GetUniqueSurveyCodeFromFeedbackId(long feedbackId);
        Task<IEnumerable<EmployerFeedbackViewModel>> GetEmployerFeedback();
        Task<IEnumerable<EmployerFeedbackAndResult>> GetAllFeedbackAndResultFromEmployer(long accountId);
        Task<int> GenerateProviderRatingResults(int allUserFeedback, int resultsforAllTime, int recentFeedbackMonths, decimal tolerance);
        Task<int> GenerateProviderAttributeResults(int allUserFeedback, int resultsforAllTime, int recentFeedbackMonths);
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummary(long ukprn);
        Task<IEnumerable<ProviderStarsSummary>> GetAllStarsSummary();
        Task<int> GetEmployerAccountIdFromUniqueSurveyCode(Guid uniqueCode);
    }
}
