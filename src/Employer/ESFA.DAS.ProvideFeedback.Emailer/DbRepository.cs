using Dapper;
using ESFA.DAS.Feedback.Employer.Emailer.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System;

namespace ESFA.DAS.Feedback.Employer.Emailer
{
    public class DbRepository
    {
        IDbConnection _dbConnection;
        IDbTransaction transaction;
        ILogger _log;

        public DbRepository(IDbConnection dbConnection,ILogger log)
        {
            _dbConnection = dbConnection;
            _dbConnection.Open();
            _log = log;
        }

        public void ReceiveDataRefreshMessage(DataRefreshMessage message)
        {
            transaction = _dbConnection.BeginTransaction();
            try
            {
                _log.LogInformation("Starting upserting users");
                UpsertIntoUsers(message.User);
                _log.LogInformation("Done upserting users\nStarting upserting providers");
                UpsertIntoProvidersAsync(message.Provider);
                _log.LogInformation("Done upserting providers\nStarting upserting feedback");
                UpsertIntoFeedbackAsync(message.User, message.Provider);
                _log.LogInformation("Done upserting feedback\nCommiting transaction");
                transaction.Commit();
                _log.LogInformation("Done commiting transaction");
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                _log.LogError("Error: " + ex.Message);
            }
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
                commandType: CommandType.StoredProcedure,
                transaction: transaction
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
                commandType: CommandType.StoredProcedure,
                transaction: transaction
            );
        }

        public async void UpsertIntoFeedbackAsync(User user, Provider provider)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserRef", user.UserRef, DbType.Guid);
            parameters.Add("@Ukprn", provider.Ukprn, DbType.Int64);
            await  _dbConnection.ExecuteAsync
            (
                sql: "[dbo].[UpsertFeedback]",
                param: parameters,
                commandType: CommandType.StoredProcedure,
                transaction: transaction
            );
        }

        public async Task<IEnumerable<FeedbackToSendResponse>> GetFeedbackToSendResponses()
        {
            var result = await _dbConnection.QueryAsync<FeedbackToSendResponse>
            (
                sql:"[dbo].[GetFeedbackToSend]",
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
