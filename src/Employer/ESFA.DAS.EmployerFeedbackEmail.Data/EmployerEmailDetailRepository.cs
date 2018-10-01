using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Esfa.Das.ProvideFeedback.Domain.Entities;

namespace ESFA.DAS.ProvideFeedback.Data
{
    public class EmployerEmailDetailRepository : IStoreEmployerEmailDetails
    {
        private int _commandTimeoutMinutes = 30;
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

        public async Task<IEnumerable<EmployerEmailDetail>> GetEmailDetailsToBeSent()
        {
            var commandTimeout = (int)TimeSpan.FromMinutes(_commandTimeoutMinutes).TotalMilliseconds;
            return await _dbConnection.QueryAsync<EmployerEmailDetail>(sql: @"
                                        SELECT * 
                                        FROM EmployerEmailDetails
                                        WHERE EmailSentDate IS NULL", param: null, transaction: null, commandTimeout: commandTimeout);
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

        public async Task SetEmailDetailsAsSent(Guid emailCode)
        {
            var now = DateTime.Now;
            await _dbConnection.QueryAsync($@"
                                UPDATE EmployerEmailDetails
                                SET EmailSentDate = @{nameof(now)}
                                WHERE EmailCode = @{nameof(emailCode)}",
                                new { now, emailCode });
        }

        public async Task SetEmailDetailsAsSent(IEnumerable<Guid> ids)
        {
            var idsArray = ids.ToArray();
            var now = DateTime.Now;
            await _dbConnection.QueryAsync($@"
                                UPDATE EmployerEmailDetails
                                SET EmailSentDate = @{nameof(now)}
                                WHERE EmailCode in @{nameof(idsArray)}",
                                new { now, idsArray});
        }
    }
}
