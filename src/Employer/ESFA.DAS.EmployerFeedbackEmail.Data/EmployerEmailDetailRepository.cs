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

        public EmployerEmailDetailRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
            _dbConnection.Open();
        }

        public async Task<EmployerEmailDetail> GetEmailDetailsForUniqueCode(Guid uniqueCode)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<EmployerEmailDetail>(
                                        $@"SELECT TOP(1) *
                                          FROM EmployerEmailDetails
                                          WHERE EmailCode = @{nameof(uniqueCode)}",
                                          new { uniqueCode });
        }

        public async Task<IEnumerable<EmployerEmailDetail>> GetEmailDetailsToBeSentInvite()
        {
            return await _dbConnection.QueryAsync<EmployerEmailDetail>(sql: @"
                                        SELECT * 
                                        FROM EmployerEmailDetails
                                        WHERE EmailSentDate IS NULL", param: null, transaction: null, commandTimeout: _commandTimeoutSeconds);
        }

        public async Task<IEnumerable<EmployerEmailDetail>> GetEmailDetailsToBeSentReminder(int minDaysSinceSent)
        {
            var minSentDate = DateTime.Now.AddDays(-minDaysSinceSent);
            return await _dbConnection.QueryAsync<EmployerEmailDetail>(sql: $@"
                                        SELECT * 
                                        FROM EmployerEmailDetails
                                        WHERE EmailSentDate IS NOT NULL
                                        AND EmailSentDate < @{nameof(minSentDate)}
                                        AND EmailReminderSentDate IS NULL
                                        AND CodeBurntDate IS NULL", param: new { minSentDate }, transaction: null, commandTimeout: _commandTimeoutSeconds);
        }

        public async Task<bool> IsCodeBurnt(Guid emailCode)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<bool>($@"
                                        SELECT CASE WHEN CodeBurntDate IS NULL THEN 0 ELSE 1 END
                                        FROM EmployerEmailDetails
                                        WHERE EmailCode = @{nameof(emailCode)}",
                                        new { emailCode });
        }

        public async Task SetCodeBurntDate(Guid uniqueCode)
        {
            var now = DateTime.Now;
            await _dbConnection.QueryAsync($@"
                                UPDATE EmployerEmailDetails
                                SET CodeBurntDate = @now
                                WHERE EmailCode = @{nameof(uniqueCode)}",
                                new { now, uniqueCode });
        }

        public async Task SetEmailDetailsAsSent(Guid userRef)
        {
            var now = DateTime.Now;
            var sql = $@"
                        UPDATE EmployerEmailDetails
                        SET EmailSentDate = @{nameof(now)}
                        WHERE UserRef = @{nameof(userRef)}
                        AND EmailSentDate IS NULL
                        AND CodeBurntDate IS NULL";

            await ExecuteUpdateAsync(sql, new { now, userRef });
        }

        public async Task SetEmailReminderAsSent(Guid userRef)
        {
            var now = DateTime.Now;
            var sql = $@"
                        UPDATE EmployerEmailDetails
                        SET EmailReminderSentDate = @{nameof(now)}
                        WHERE UserRef = @{nameof(userRef)}
                        AND EmailReminderSentDate IS NULL
                        AND CodeBurntDate IS NULL";

            await ExecuteUpdateAsync(sql, new { now, userRef });
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
    }
}
