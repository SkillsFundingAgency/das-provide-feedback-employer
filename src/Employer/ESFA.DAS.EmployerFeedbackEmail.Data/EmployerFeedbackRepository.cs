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

        public EmployerFeedbackRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
            _dbConnection.Open();
        }

        public async Task<EmployerSurveyInvite> GetEmployerInviteForUniqueCode(Guid uniqueCode)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<EmployerSurveyInvite>(
                                        $@"SELECT TOP(1) *
                                          FROM {EmployerSurveyInvites}
                                          WHERE UniqueSurveyCode = @{nameof(uniqueCode)}",
                                          new { uniqueCode });
        }

        public async Task<IEnumerable<EmployerSurveyInvite>> GetEmployerUsersToBeSentInvite(int minDaysSincePreviousSurvey)
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
                                        FROM {EmployerSurveyInvites}
                                        WHERE UniqueSurveyCode = @{nameof(emailCode)}",
                                        new { emailCode });
        }

        public async Task SetCodeBurntDate(Guid uniqueCode)
        {
            var now = DateTime.Now;
            await _dbConnection.QueryAsync($@"
                                UPDATE {EmployerSurveyInvites}
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
                    x.Ukprn
                };
            });

            var sql = $@"
                        INSERT INTO EmployerSurveyCodes
                        VALUES(@UniqueSurveyCode, @UserRef, @Ukprn, NULL)";

            await _dbConnection.ExecuteAsync(sql, newCodesToCreate);
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

        public async void UpsertIntoUsers(User user)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserRef", user.UserRef, DbType.Guid);
            parameters.Add("@FirstName", user.FirstName, DbType.String);
            parameters.Add("@EmailAddress", user.EmailAddress, DbType.String);
            parameters.Add("@AccountId", user.AccountId, DbType.Int64);
            await _dbConnection.ExecuteAsync
            (
                sql: "[dbo].[UpsertUsers]",
                param: parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async void UpsertIntoProvidersAsync(Provider provider)
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

        public async void UpsertIntoFeedbackAsync(User user, Provider provider)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserRef", user.UserRef, DbType.Guid);
            parameters.Add("@Ukprn", provider.Ukprn, DbType.Int64);
            await _dbConnection.ExecuteAsync
            (
                sql: "[dbo].[UpsertFeedback]",
                param: parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<FeedbackToSendResponse>> GetFeedbackToSendResponses()
        {
            var result = await _dbConnection.QueryAsync<FeedbackToSendResponse>
            (
                sql: "[dbo].[GetFeedbackToSend]",
                commandType: CommandType.StoredProcedure
            );
            return result;
        }

        public async void ResetFeedback()
        {
            await _dbConnection.ExecuteAsync
            (
                sql: "[dbo].[ResetFeedback]",
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
