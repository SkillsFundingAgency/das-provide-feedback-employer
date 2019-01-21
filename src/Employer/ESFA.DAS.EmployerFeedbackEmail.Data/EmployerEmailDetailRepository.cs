using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ESFA.DAS.ProvideFeedback.Domain.Entities;
using Polly;

namespace ESFA.DAS.ProvideFeedback.Data
{
    public class EmployerEmailDetailRepository : IStoreEmployerEmailDetails
    {
        private int _commandTimeoutSeconds = 120;
        private readonly IDbConnection _dbConnection;
        private const string EmployerSurveyInvites = "vw_EmployerSurveyInvites";

        public EmployerEmailDetailRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
            _dbConnection.Open();
        }

        public async Task<EmployerSurveyInvite> GetEmailDetailsForUniqueCode(Guid uniqueCode)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<EmployerSurveyInvite>(
                                        $@"SELECT TOP(1) *
                                          FROM {EmployerSurveyInvites}
                                          WHERE UniqueSurveyCode = @{nameof(uniqueCode)}",
                                          new { uniqueCode });
        }

        public async Task<IEnumerable<EmployerSurveyInvite>> GetEmailDetailsToBeSentInvite(int minDaysSincePreviousSurvey)
        {
            var minAllowedSendDate = DateTime.Now.AddDays(-minDaysSincePreviousSurvey);
            return await _dbConnection.QueryAsync<EmployerSurveyInvite>(sql: $@"
                                        SELECT * 
                                        FROM {EmployerSurveyInvites}
                                        WHERE InviteSentDate IS NULL", commandTimeout: _commandTimeoutSeconds);
        }

        public async Task<IEnumerable<EmployerSurveyInvite>> GetEmailDetailsToBeSentReminder(int minDaysSinceInvite)
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

        //public async Task SetEmailReminderAsSent(Guid userRef)
        //{
        //    var now = DateTime.Now;
        //    var sql = $@"
        //                UPDATE EmployerEmailDetails
        //                SET EmailReminderSentDate = @{nameof(now)}
        //                WHERE UserRef = @{nameof(userRef)}
        //                AND (CodeBurntDate IS NULL
        //                OR CodeBurntDate < EmailSentDate)";

        //    await ExecuteUpdateAsync(sql, new { now, userRef });
        //}

        private async Task ExecuteUpdateAsync(string sql, object param)
        {
            var policy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(3, x => TimeSpan.FromSeconds(3));

            await policy.ExecuteAsync(() =>
            {
                return _dbConnection.QueryAsync(sql: sql, param: param, transaction: null, commandTimeout: _commandTimeoutSeconds);
            });
        }
    }
}
