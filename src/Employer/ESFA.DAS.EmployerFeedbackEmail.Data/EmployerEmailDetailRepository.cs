using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Esfa.Das.ProvideFeedback.Domain.Entities;

namespace ESFA.DAS.ProvideFeedback.Data
{
    public class EmployerEmailDetailRepository : IStoreEmployerEmailDetails
    {
        private readonly IDbConnection _dbConnection;

        public EmployerEmailDetailRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
            _dbConnection.Open();
        }

        public async Task<EmployerEmailDetail> GetEmailDetailsForUniqueCode(Guid guid)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<EmployerEmailDetail>(
                                        @"SELECT TOP(1) *
                                          FROM EmployerEmailDetails
                                          WHERE EmailUID = @guid",
                                          new { guid });
        }

        public async Task<IEnumerable<EmployerEmailDetail>> GetEmailDetailsToBeSent()
        {
            return await _dbConnection.QueryAsync<EmployerEmailDetail>(@"
                                        SELECT * 
                                        FROM EmployerEmailDetails
                                        WHERE EmailSentDate IS NULL");
        }

        public async Task<bool> IsCodeBurnt(Guid emailCode)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<bool>(@"
                                        SELECT CASE WHEN CodeBurntDate IS NULL THEN 0 ELSE 1 END
                                        FROM EmployerEmailDetails");
        }

        public async Task SetCodeBurntDate()
        {
            var now = DateTime.Now;
            await _dbConnection.QueryAsync($@"
                                UPDATE EmployerEmailDetails
                                SET CodeBurntDate = @{nameof(now)}", 
                                new { now });
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

        public Task SetEmailDetailsAsSent(IEnumerable<Guid> id)
        {
            throw new NotImplementedException();
        }
    }
}
