using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using ESFA.DAS.ProvideFeedback.Data.Enums;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;

namespace ESFA.DAS.ProvideFeedback.Data.Repositories
{
    public class EmployerFeedbackRepository : IEmployerFeedbackRepository
    {
        private int _commandTimeoutSeconds = 120;
        private readonly IDbConnection _dbConnection;
        private const string EmployerSurveyCodes = "EmployerSurveyCodes";

        public EmployerFeedbackRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<int> GetEmployerAccountIdFromUniqueSurveyCode(Guid uniqueCode)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<int>(
                                        $@"SELECT AccountId
                                        FROM EmployerSurveyCodes ESC
                                        JOIN EmployerFeedback EF ON ESC.FeedbackId = EF.FeedbackId
                                        WHERE ESC.UniqueSurveyCode = @{nameof(uniqueCode)}",
                                        new { uniqueCode }
                                        );
        }

        public async Task<bool> IsCodeBurnt(Guid emailCode)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<bool>($@"
                                        SELECT CASE WHEN BurnDate IS NULL THEN 0 ELSE 1 END
                                        FROM {EmployerSurveyCodes}
                                        WHERE UniqueSurveyCode = @{nameof(emailCode)}",
                                        new { emailCode });
        }

        public async Task SetCodeBurntDate(Guid uniqueCode)
        {
            var now = DateTime.UtcNow;
            await _dbConnection.QueryAsync($@"
                                UPDATE {EmployerSurveyCodes}
                                SET BurnDate = @now
                                WHERE UniqueSurveyCode = @{nameof(uniqueCode)}",
                                new { now, uniqueCode });
        }

        public async Task<DateTime?> GetCodeBurntDate(Guid uniqueCode)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<DateTime?>($@"
                                       SELECT BurnDate 
                                       FROM {EmployerSurveyCodes} 
                                       WHERE UniqueSurveyCode = @{nameof(uniqueCode)}",
                                       new { uniqueCode });
        }

        public async Task UpsertIntoUsers(User user)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserRef", user.UserRef, DbType.Guid);
            parameters.Add("@EmailAddress", user.EmailAddress, DbType.String);
            parameters.Add("@FirstName", user.FirstName, DbType.String);

            await _dbConnection.ExecuteAsync
            (
                sql: "[dbo].[UpsertUsers]",
                param: parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: _commandTimeoutSeconds
            );
        }

        public async Task UpsertIntoProviders(IEnumerable<Provider> providers)
        {
            var providerDt = ProvidersToDatatable(providers);

            await _dbConnection.ExecuteAsync
            (
                sql: "[dbo].[UpsertProviders]",
                param: new { ProvidersDt = providerDt.AsTableValuedParameter("ProviderTemplate") },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<long> UpsertIntoFeedback(Guid userRef, long accountId, long ukprn)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserRef", userRef, DbType.Guid);
            parameters.Add("@Ukprn", ukprn, DbType.Int64);
            parameters.Add("@AccountId", accountId, DbType.Int64);

            var feedbackId = await _dbConnection.ExecuteScalarAsync<long>
            (
                sql: "[dbo].[UpsertFeedback]",
                param: parameters,
                commandType: CommandType.StoredProcedure
            );

            return feedbackId;
        }

        public async Task<IEnumerable<FeedbackQuestionAttribute>> GetAllAttributes()
        {
            return await _dbConnection.QueryAsync<FeedbackQuestionAttribute>("SELECT * FROM Attributes");
        }

        public async Task<Guid> GetUniqueSurveyCodeFromFeedbackId(long feedbackId)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<Guid>("SELECT UniqueSurveyCode FROM EmployerSurveyCodes WHERE FeedbackId = @feedbackId"
                , new { feedbackId = feedbackId });
        }

        public async Task<Guid> CreateEmployerFeedbackResult(long feedbackId, string providerRating, DateTime dateTimeCompleted, FeedbackSource feedbackSource, IEnumerable<ProviderAttribute> providerAttributes)

        {
            var providerAttributesDt = ProviderAttributesToDataTable(providerAttributes);

            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }
            var transaction = _dbConnection.BeginTransaction();
            try
            {
                var parameterTemplate = new
                {
                    FeedbackId = feedbackId,
                    ProviderRating = providerRating,
                    FeedbackSource = (int)feedbackSource,
                    ProviderAttributesDt = providerAttributesDt.AsTableValuedParameter("ProviderAttributesTemplate")
                };
                var parameters = new DynamicParameters(parameterTemplate);
                parameters.Add("@DateTimeCompleted", dateTimeCompleted, DbType.DateTime2);

                var result = await _dbConnection.QueryFirstOrDefaultAsync<Guid>(

                    sql: "[dbo].[CreateEmployerFeedbackResult]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: transaction
                );

                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                _dbConnection.Close();
            }
        }

        public async Task<EmployerFeedback> GetEmployerFeedbackRecord(Guid userRef, long accountId, long ukprn)
        {
            return await _dbConnection.
                QueryFirstOrDefaultAsync<EmployerFeedback>(@"SELECT TOP 1 * FROM EmployerFeedback WHERE UserRef = @userRef AND Ukprn = @ukprn AND AccountId = @accountId",
                new
                {
                    userRef,
                    accountId,
                    ukprn
                });
        }

        public async Task<IEnumerable<EmployerFeedbackViewModel>> GetEmployerFeedback()
        {
            return await _dbConnection.QueryAsync<EmployerFeedbackViewModel>(
                @"SELECT efr.Id, ef.Ukprn, efr.FeedbackId, efr.DateTimeCompleted, efr.ProviderRating, a.AttributeName, pa.AttributeValue
                  FROM EmployerFeedback ef JOIN EmployerFeedbackResult efr on ef.FeedbackId = efr.FeedbackId  LEFT JOIN ProviderAttributes pa on efr.Id = pa.EmployerFeedbackResultId
                  LEFT JOIN Attributes a on pa.AttributeId = a.AttributeId");
        }

        public async Task<IEnumerable<EmployerFeedbackAndResult>> GetAllFeedbackAndResultFromEmployer(long accountId)
        {
            return await _dbConnection.
                QueryAsync<EmployerFeedbackAndResult>(@"SELECT * FROM EmployerFeedback ef INNER JOIN EmployerFeedbackResult efr ON ef.FeedbackId = efr.FeedbackId WHERE ef.AccountId = @accountId ORDER BY DateTimeCompleted DESC",
                new
                {
                    accountId,
                });
        }

        public async Task<int> GenerateProviderRatingResults(int allUserFeedback, int resultsforAllTime, int recentFeedbackMonths, decimal tolerance)
        {
            if (_dbConnection.State == ConnectionState.Closed)
                _dbConnection.Open();
            var parameters = new DynamicParameters();
            parameters.Add("@AllUserFeedback", allUserFeedback, DbType.Int64);
            parameters.Add("@ResultsforAllTime", resultsforAllTime, DbType.Int64);
            parameters.Add("@RecentFeedbackMonths", recentFeedbackMonths, DbType.Int64);
            parameters.Add("@Tolerance", tolerance, DbType.Decimal);

            return await _dbConnection.ExecuteAsync(
                commandType: CommandType.StoredProcedure,
                sql: "[dbo].[GenerateProviderRatingResults]",
                param: parameters,
                commandTimeout: _commandTimeoutSeconds);
        }

        public async Task<int> GenerateProviderAttributeResults(int allUserFeedback, int resultsforAllTime, int recentFeedbackMonths)
        {
            if (_dbConnection.State == ConnectionState.Closed)
                _dbConnection.Open();
            var parameters = new DynamicParameters();
            parameters.Add("@AllUserFeedback", allUserFeedback, DbType.Int64);
            parameters.Add("@ResultsforAllTime", resultsforAllTime, DbType.Int64);
            parameters.Add("@RecentFeedbackMonths", recentFeedbackMonths, DbType.Int64);

            return await _dbConnection.ExecuteAsync(
                commandType: CommandType.StoredProcedure,
                sql: "[dbo].[GenerateProviderAttributeResults]",
                param: parameters,
                commandTimeout: _commandTimeoutSeconds);
        }

        public async Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummary(long ukprn)
        {
            return await _dbConnection.
                QueryAsync<EmployerFeedbackResultSummary>(@"SELECT pss.Ukprn, pss.ReviewCount, pss.Stars, a.AttributeName, pas.Strength, pas.Weakness, pas.UpdatedOn FROM ProviderStarsSummary pss LEFT JOIN ProviderAttributeSummary pas ON pss.Ukprn = pas.Ukprn LEFT JOIN Attributes a ON pas.AttributeId = a.AttributeId WHERE pss.Ukprn = @ukprn",
                new
                {
                    ukprn,
                });
        }

        public async Task<IEnumerable<ProviderStarsSummary>> GetAllStarsSummary()
        {
            return await _dbConnection.
                QueryAsync<ProviderStarsSummary>(@"SELECT Ukprn, ReviewCount, Stars FROM ProviderStarsSummary");
        }

        private DataTable ProvidersToDatatable(IEnumerable<Provider> providers)
        {
            var dt = new DataTable();
            dt.Columns.Add("Ukprn", typeof(long));
            dt.Columns.Add("ProviderName", typeof(string));

            foreach (var provider in providers)
            {
                dt.Rows.Add(provider.Ukprn, provider.ProviderName);
            }

            return dt;
        }

        private DataTable ProviderAttributesToDataTable(IEnumerable<ProviderAttribute> providerAttributes)
        {
            var dt = new DataTable();
            dt.Columns.Add("AttributeId", typeof(long));
            dt.Columns.Add("AttributeValue", typeof(int));

            foreach (var providerAttribute in providerAttributes)
            {
                dt.Rows.Add(providerAttribute.AttributeId, providerAttribute.AttributeValue);
            }

            return dt;
        }
    }
}
