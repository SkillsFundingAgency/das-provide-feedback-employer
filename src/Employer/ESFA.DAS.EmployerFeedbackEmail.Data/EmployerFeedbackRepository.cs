using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using ESFA.DAS.ProvideFeedback.Domain.Entities;

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
            return await _dbConnection.QueryAsync<EmployerSurveyInvite>(sql: $@"
                                        SELECT * 
                                        FROM {EmployerSurveyInvites}
                                        WHERE InviteSentDate IS NULL", commandTimeout: _commandTimeoutSeconds);
        }

        public async Task<IEnumerable<EmployerSurveyInvite>> GetEmployerInvitesToBeSentReminder(int minDaysSinceInvite)
        {
            var minSentDate = DateTime.Now.AddDays(-minDaysSinceInvite);
            return await _dbConnection.QueryAsync<EmployerSurveyInvite>(sql: $@"
                                        SELECT * 
                                        FROM {EmployerSurveyInvites}
                                        WHERE InviteSentDate IS NOT NULL
                                        AND LastReminderSentDate IS NULL
                                        AND InviteSentDate < @{nameof(minSentDate)}
                                        AND CodeBurntDate IS NULL", param: new { minSentDate }, transaction: null, commandTimeout: _commandTimeoutSeconds);
        }

        public async Task<bool> IsCodeBurnt(Guid emailCode)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<bool>($@"
                                        SELECT CASE WHEN CodeBurntDate IS NULL THEN 0 ELSE 1 END
                                        FROM {EmployerSurveyCodes}
                                        WHERE UniqueSurveyCode = @{nameof(emailCode)}",
                                        new { emailCode });
        }

        public async Task SetCodeBurntDate(Guid uniqueCode)
        {
            var now = DateTime.Now;
            await _dbConnection.QueryAsync($@"
                                UPDATE {EmployerSurveyCodes}
                                SET CodeBurntDate = @now
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

        public async Task<IEnumerable<EmployerSurveyInvite>> GetEmployerInvitesForNextCycleAsync(int inviteCycleDays)
        {
            var minAllowedSendDate = DateTime.Now.AddDays(-inviteCycleDays);
            return await _dbConnection.QueryAsync<EmployerSurveyInvite>($@"
                            SELECT * FROM {EmployerSurveyInvites}
                            WHERE InviteSentDate < @{nameof(minAllowedSendDate)}", new { minAllowedSendDate });
        }

        public async Task InsertNewSurveyInviteCodes(IEnumerable<EmployerSurveyInvite> newCodesRequired)
        {
            var newCodesToCreate = newCodesRequired.Select(x =>
            {
                return new
                {
                    UniqueSurveyCode = Guid.NewGuid(),
                    FeedbackId = GetFeedbackId(x.UserRef,x.Ukprn,x.AccountId).Result
                };
            });

            var sql = $@"
                        INSERT INTO {EmployerSurveyCodes}
                        VALUES(@UniqueSurveyCode, @FeedbackId, NULL)";

            await _dbConnection.ExecuteAsync(sql, newCodesToCreate);
        }

        private async Task<long> GetFeedbackId(Guid UserRef, long Ukprn, long AccountId)
        {
            return await _dbConnection.QuerySingleAsync<long>($@"
            SELECT FeedbackID FROM EmployerFeedback WHERE UserRef = @UserRef AND Ukprn = @Ukprn AND AccountId = @AccountID", new {UserRef,Ukprn,AccountId });
        }

        public async Task<Guid> GetOrCreateSurveyCode(Guid UserRef, long Ukprn, long AccountId)
        {
            var sql = $@"
                        SELECT UniqueSurveyCode FROM {EmployerSurveyCodes}
                        WHERE FeedbackId = (SELECT FeedbackId FROM EmployerFeedback WHERE UserRef = @UserRef AND Ukprn = @Ukprn AND AccountId = @AccountId)";
            var result = await _dbConnection.QueryAsync<Guid>(sql,new{UserRef,Ukprn, AccountId });
            if (result.Any())
            {
                return result.Single();
            }

            var newCode = new
            {
                UniqueSurveyCode = Guid.NewGuid(),
                UserRef,
                Ukprn,
                AccountId
            };

            var sql2 = $@"
                        INSERT INTO {EmployerSurveyCodes}
                        VALUES(@UniqueSurveyCode,(SELECT FeedbackId FROM EmployerFeedback WHERE UserRef = @UserRef AND Ukprn = @Ukprn AND AccountId = @AccountId), NULL)";

            await _dbConnection.ExecuteAsync(sql2, newCode);
            return newCode.UniqueSurveyCode;
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

        public async Task UpsertIntoProvidersAsync(Provider provider)
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

        public async Task UpsertIntoFeedbackAsync(User user, Provider provider)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserRef", user.UserRef, DbType.Guid);
            parameters.Add("@Ukprn", provider.Ukprn, DbType.Int64);
            parameters.Add("@AccountId", user.AccountId, DbType.Int64);
            await _dbConnection.ExecuteAsync
            (
                sql: "[dbo].[UpsertFeedback]",
                param: parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task ResetFeedback()
        {
            await _dbConnection.ExecuteAsync
            (
                sql: "[dbo].[ResetFeedback]",
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task ClearSurveyCodes(Guid userRef)
        {
            await _dbConnection.ExecuteAsync($@"
            DELETE FROM EmployerSurveyHistory where uniqueSurveyCode in (SELECT UniqueSurveyCode from EmployerSurveyCodes where FeedbackId in (SELECT FeedbackId FROM EmployerFeedback WHERE UserRef = @userRef))
            DELETE FROM {EmployerSurveyCodes} where FeedbackId in (SELECT FeedbackId FROM EmployerFeedback WHERE UserRef = @userRef)", new{userRef});
        }
    }
}
