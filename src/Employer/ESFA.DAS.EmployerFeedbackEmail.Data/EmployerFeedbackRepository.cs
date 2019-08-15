using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;

namespace ESFA.DAS.ProvideFeedback.Data
{
    public class EmployerFeedbackRepository : IStoreEmployerEmailDetails
    {
        private int _commandTimeoutSeconds = 120;
        private readonly IDbConnection _dbConnection;
        private const string EmployerSurveyInvites = "vw_EmployerSurveyInvites";
        private const string EmployerSurveyCodes = "EmployerSurveyCodes";
        private const string EmployerSurveyHistoryComplete = "vw_EmployerSurveyHistoryComplete";

        public EmployerFeedbackRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
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
            var minSentDate = DateTime.Now.AddDays(-minDaysSinceInvite);
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
            var now = DateTime.Now;
            await _dbConnection.QueryAsync($@"
                                UPDATE {EmployerSurveyCodes}
                                SET BurnDate = @now
                                WHERE UniqueSurveyCode = @{nameof(uniqueCode)}",
                                new { now, uniqueCode });
        }

        public async Task InsertSurveyInviteHistory(IEnumerable<Guid> uniqueSurveyCodes, int inviteType)
        {
            var now = DateTime.Now;
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
            parameters.Add("@FirstName", user.FirstName, DbType.String);
            parameters.Add("@EmailAddress", user.EmailAddress, DbType.String);
            await _dbConnection.ExecuteAsync
            (
                sql: "[dbo].[UpsertUsers]",
                param: parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task UpsertIntoProviders(Provider provider)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Ukprn", provider.Ukprn, DbType.Int64);
            parameters.Add("@ProviderName", provider.ProviderName, DbType.String);
            await _dbConnection.ExecuteAsync
            (
                sql: "[dbo].[UpsertProviders]",
                param: parameters,
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
                commandType: CommandType.StoredProcedure
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
    }
}
