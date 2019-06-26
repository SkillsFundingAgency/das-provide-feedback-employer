using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using ESFA.DAS.ProvideFeedback.Domain.Entities;
using Polly;

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
                    x.UserRef, 
                    x.Ukprn,
                    x.AccountId
                };
            });

            var sql = $@"
                        INSERT INTO {EmployerSurveyCodes}
                        VALUES(@UniqueSurveyCode, @UserRef, @Ukprn, @AccountId, NULL)";

            await _dbConnection.ExecuteAsync(sql, newCodesToCreate);
        }

        public async Task<Guid> GetOrCreateSurveyCode(Guid UserRefParam, long UkprnParam, long AccountIdParam)
        {
            var sql = $@"
                        SELECT UniqueSurveyCode FROM {EmployerSurveyCodes}
                        WHERE UserRef = @UserRefParam AND Ukprn = @UkprnParam AND AccountId = @AccountIdParam";
            var result = await _dbConnection.QueryAsync<Guid>(sql,new{UserRefParam,UkprnParam, AccountIdParam });
            if (result.Count() == 0)
            {
                var newCode = new
                {
                    UniqueSurveyCode = Guid.NewGuid(),
                    UserRef = UserRefParam,
                    Ukprn = UkprnParam,
                    AccountId = AccountIdParam
                };

                var sql2 = $@"
                        INSERT INTO {EmployerSurveyCodes}
                        VALUES(@UniqueSurveyCode, @UserRef, @Ukprn, @AccountId, NULL)";

                await _dbConnection.ExecuteAsync(sql2, newCode);
                return newCode.UniqueSurveyCode;
            }
            else
            {
                return result.Single();
            }
        }


        private async Task ExecuteUpdateAsync(string sql, object param)
        {
            var policy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(3, x => TimeSpan.FromSeconds(3));

            await policy.ExecuteAsync(() =>
            {
                return _dbConnection.QueryAsync(sql: sql, param: param, transaction: null, commandTimeout: _commandTimeoutSeconds);
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
            DELETE FROM EmployerSurveyHistory where uniqueSurveyCode in (SELECT UniqueSurveyCode from EmployerSurveyCodes where UserRef = @userRef)
            DELETE FROM {EmployerSurveyCodes} where UserRef = @userRef", new{userRef});
        }
    }
}
