using Dapper;
using ESFA.DAS.Feedback.Employer.Emailer.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ESFA.DAS.Feedback.Employer.Emailer
{
    public class DbRepository
    {
        IDbConnection _dbConnection;

        public DbRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
            _dbConnection.Open();
        }

        public void ReceiveDataRefreshMessage(DataRefreshMessage message)
        {
            UpsertIntoUsers(message.User);
            UpsertIntoProvidersAsync(message.Provider);
            UpsertIntoFeedbackAsync(message.User, message.Provider);
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
            await  _dbConnection.ExecuteAsync
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
