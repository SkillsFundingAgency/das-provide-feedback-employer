using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;

namespace ESFA.DAS.ProvideFeedback.Employer.Application
{
    public class EmployerFeedbackDataRetrievalService
    {
        IEmployerCommitmentApi _commitmentApiClient;
        IAccountApiClient _accountApiClient;
        IStoreEmployerEmailDetails _emailDetailsRepository;

        public EmployerFeedbackDataRetrievalService(
            IEmployerCommitmentApi commitmentApiClient,
            IAccountApiClient accountApiClient,
            IStoreEmployerEmailDetails emailDetailsRepository)
        {
            _commitmentApiClient = commitmentApiClient;
            _accountApiClient = accountApiClient;
            _emailDetailsRepository = emailDetailsRepository;
        }

        public async Task<IEnumerable<EmployerFeedbackRefreshMessage>> GetRefreshData(long accountId)
        {
            var mappedUsersTask = GetAccountUsersForFeedback(accountId);

            var accCommitmentsTask = _commitmentApiClient.GetEmployerApprenticeships(accountId);

            var accCommitments = await accCommitmentsTask;

            var commitmentUkprns = accCommitments
                .GroupBy(acc => acc.ProviderId)
                .Select(group => group.Key);

            var providers = await _emailDetailsRepository.GetProvidersByUkprn(commitmentUkprns);

            var validCommitments = accCommitments
                .Where(app => app != null)
                .Where(app => app.HasHadDataLockSuccess)
                .Where(app => app.PaymentStatus == PaymentStatus.Active || app.PaymentStatus == PaymentStatus.Paused)
                .Where(app => providers.Any(p => p.Ukprn == app.ProviderId))
                .GroupBy(app => new { app.EmployerAccountId, app.ProviderId })
                .Select(app => app.First());

            var mappedUsers = await mappedUsersTask;

            var messages = validCommitments
                .SelectMany(c => mappedUsers.Select(mu => new EmployerFeedbackRefreshMessage
                {
                    ProviderId = c.ProviderId,
                    User = mu
                }));

            return messages;
        }

        private async Task<IEnumerable<User>> GetAccountUsersForFeedback(long accountId)
        {
            var users = await _accountApiClient.GetAccountUsers(accountId);
            return users
                .Where(au => au.CanReceiveNotifications)
                .Select(au => MapTeamMemberToUser(au, accountId));
        }

        private User MapTeamMemberToUser(TeamMemberViewModel tmvw, long AccountId)
        {
            return new User
            {
                EmailAddress = tmvw.Email,
                UserRef = Guid.Parse(tmvw.UserRef),
                AccountId = AccountId,
                FirstName = tmvw.Name.Split(' ')[0]
            };
        }
    }
}
