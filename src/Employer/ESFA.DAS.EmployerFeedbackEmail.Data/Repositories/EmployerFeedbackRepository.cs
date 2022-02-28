using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;

namespace ESFA.DAS.ProvideFeedback.Data.Repositories
{
    public class EmployerFeedbackRepository : IEmployerFeedbackRepository
    {
        private int _commandTimeoutSeconds = 120;
        private readonly IDbConnection _dbConnection;
        private const string EmployerSurveyCodes = "EmployerSurveyCodes";
        private const string EmployerSurveyHistoryComplete = "vw_EmployerSurveyHistoryComplete";

        public EmployerFeedbackRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public Task<FeedbackInvite> GetLatestFeedbackInviteSentDateAsync(long feedbackId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@FeedbackId", feedbackId, DbType.Int64);
            return _dbConnection.QuerySingleAsync<FeedbackInvite>(
                commandType: CommandType.StoredProcedure,
                sql: "[dbo].[GetLatestFeedbackInviteSentDate]",
                param: parameters,
                commandTimeout: _commandTimeoutSeconds);
        }

        public async Task<EmployerSurveyInvite> GetEmployerInviteForUniqueCode(Guid uniqueCode)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<EmployerSurveyInvite>(
                                        $@"SELECT TOP(1) *
                                          FROM {EmployerSurveyHistoryComplete}
                                          WHERE UniqueSurveyCode = @{nameof(uniqueCode)}",
                                          new { uniqueCode });
        }

        public async Task<IEnumerable<EmployerSurveyInvite>> GetEmployerUsersToBeSentInvite()
        {
            return await _dbConnection.QueryAsync<EmployerSurveyInvite>(
                sql: "[dbo].[GetSurveyInvitesToSend]",
                commandType: CommandType.StoredProcedure,
                commandTimeout: _commandTimeoutSeconds);
        }

        public async Task<IEnumerable<EmployerSurveyInvite>> GetEmployerInvitesToBeSentReminder(int minDaysSinceInvite)
        {
            var minSentDate = DateTime.UtcNow.AddDays(-minDaysSinceInvite);
            var parameters = new DynamicParameters();
            parameters.Add("@MinSentDate", minSentDate, DbType.DateTime2);
            return await _dbConnection.QueryAsync<EmployerSurveyInvite>(
                sql: "[dbo].[GetSurveyRemindersToSend]",
                param: parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: _commandTimeoutSeconds);
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

        public async Task InsertSurveyInviteHistory(IEnumerable<Guid> uniqueSurveyCodes, int inviteType)
        {
            var now = DateTime.UtcNow;
            var toInsert = uniqueSurveyCodes.Select(uniqueSurveyCode =>
            {
                return new
                {
                    uniqueSurveyCode,
                    inviteType,
                    now
                };
            });

            var sql = $@"
                        INSERT INTO EmployerSurveyHistory
                        VALUES(@uniqueSurveyCode, @inviteType, @now)";

            await _dbConnection.ExecuteAsync(sql, toInsert);
        }

        public async Task InsertNewSurveyForFeedback(long feedbackId)
        {
            var sql = $@"
                        INSERT INTO {EmployerSurveyCodes}
                        VALUES(@UniqueSurveyCode, @FeedbackId, NULL)";

            await _dbConnection.ExecuteAsync(
                sql,
                new
                {
                    UniqueSurveyCode = Guid.NewGuid(),
                    FeedbackId = feedbackId
                });
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

        public async Task ResetFeedback()
        {
            await _dbConnection.ExecuteAsync
            (
                sql: "[dbo].[ResetFeedback]",
                commandType: CommandType.StoredProcedure,
                commandTimeout: _commandTimeoutSeconds
            );
        }

        public async Task<EmployerSurveyInvite> GetEmployerSurveyInvite(long feedbackId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@FeedbackId", feedbackId, DbType.Int64);
            return await _dbConnection.QueryFirstOrDefaultAsync<EmployerSurveyInvite>
                (
                    sql: "[dbo].[GetEmployerSurveyHistory]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                );
        }

        public async Task MarkProviderInactive()
        {
            await _dbConnection.QueryAsync("UPDATE Providers SET IsActive = 0");
        }

        public async Task<IEnumerable<Provider>> GetProvidersByUkprn(IEnumerable<long> ukprns)
        {
            var sql = @"SELECT * FROM Providers
                        WHERE IsActive = 1
                        AND Ukprn in @ukprns";

            return await _dbConnection.QueryAsync<Provider>(sql, new { ukprns });
        }

        public async Task<Provider> GetProviderByUkprn(long ukprn)
        {
            var sql = @"SELECT TOP 1 * FROM Providers WHERE Ukprn = @ukprn";
            return await _dbConnection.QueryFirstOrDefaultAsync<Provider>(sql, new { ukprn });
        }

        public async Task<User> GetUserByUserRef(Guid userRef)
        {
            var sql = @"SELECT TOP 1 * FROM Users WHERE UserRef = @userRef";
            return await _dbConnection.QueryFirstOrDefaultAsync<User>(sql, new { userRef });
        }

        public async Task<IEnumerable<FeedbackQuestionAttribute>> GetAllAttributes()
        {
            return await _dbConnection.QueryAsync<FeedbackQuestionAttribute>("SELECT * FROM Attributes");
        }

        public async Task<long> GetFeedbackIdFromUniqueSurveyCode(Guid uniqueSurveyCode)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<long>("SELECT FeedbackId FROM EmployerSurveyCodes WHERE UniqueSurveyCode = @uniqueSurveyCode"
                , new { uniqueSurveyCode = uniqueSurveyCode });
        }

        public async Task<Guid> CreateEmployerFeedbackResult(long feedbackId, string providerRating, DateTime dateTimeCompleted, IEnumerable<ProviderAttribute> providerAttributes)
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

        public async Task<EmployerFeedbackResult> GetEmployerFeedbackResultRecord(long feedbackId, DateTime datetimeCompleted)
        {
            var parameterTemplate = new
            {
                feedbackId
            };
            var parameters = new DynamicParameters(parameterTemplate);
            parameters.Add("@dateTimeCompleted", datetimeCompleted, DbType.DateTime2);
            
            return await _dbConnection.
                QueryFirstOrDefaultAsync<EmployerFeedbackResult>(@"SELECT TOP 1 * FROM EmployerFeedbackResult WHERE feedbackId = @feedbackId AND dateTimeCompleted = @dateTimeCompleted", parameters);
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
