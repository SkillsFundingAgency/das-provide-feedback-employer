using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using SFA.DAS.EAS.Account.Api.Types;

namespace ESFA.DAS.ProvideFeedback.Employer.Application
{
    public class EmployerFeedbackDataRetrievalService
    {
        ICommitmentService _commitmentService;
        IAccountService _accountService;
        IStoreEmployerEmailDetails _emailDetailsRepository;

        public EmployerFeedbackDataRetrievalService(
            ICommitmentService commitmentService,
            IAccountService accountService,
            IStoreEmployerEmailDetails emailDetailsRepository)
        {
            _commitmentService = commitmentService;
            _accountService = accountService;
            _emailDetailsRepository = emailDetailsRepository;
        }

        public async Task<IEnumerable<EmployerFeedbackRefreshMessage>> GetRefreshData(long accountId)
        {
            var mappedUsersTask = GetAccountUsersForFeedback(accountId);

            var accCommitments = await _commitmentService.GetApprenticeships(accountId).ConfigureAwait(false);

            var commitmentUkprns = accCommitments.Apprenticeships
                .GroupBy(acc => acc.ProviderId)
                .Select(group => group.Key);

            var providers = await _emailDetailsRepository.GetProvidersByUkprn(commitmentUkprns);

            var validCommitments = accCommitments
                .Apprenticeships
                .Where(app => providers.Any(p => p.Ukprn == app.ProviderId))
            .GroupBy(app => new { app.ProviderId })
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
            var users = await _accountService.GetAccountUsers(accountId);
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
