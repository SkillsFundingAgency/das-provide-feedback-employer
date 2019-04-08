using ESFA.DAS.Feedback.Employer.Emailer.Models;
using SFA.DAS.Apprenticeships.Api.Types.Providers;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Providers.Api.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using Provider = ESFA.DAS.Feedback.Employer.Emailer.Models.Provider;

namespace ESFA.DAS.Feedback.Employer.Emailer
{
    public class EmailInviteDataRefresh
    {
        IProviderApiClient _providerApiClient;
        IEmployerCommitmentApi _commitmentApiClient;
        IAccountApiClient _accountApiClient;

        public EmailInviteDataRefresh(IProviderApiClient providerApiClient, IEmployerCommitmentApi commitmentApiClient, IAccountApiClient accountApiClient)
        {
            _providerApiClient = providerApiClient;
            _commitmentApiClient = commitmentApiClient;
            _accountApiClient = accountApiClient;
        }

        public List<DataRefreshMessage> GetRefreshData()
        {
            var apprenticeships = GetValidApprenticeshipCommitments(GetApprenticeshipData());
            var messages = new List<DataRefreshMessage>();
            messages.AddRange(apprenticeships.SelectMany(Combine));

            //Send Message Or Whatever
            return messages;
        }

        public IEnumerable<ProviderSummary> GetRoatpProviders()
        {
            var result = _providerApiClient.FindAll();
            return result;
        }

        public IEnumerable<long> GetCommitmentEmployerIdsData()
        {
            return _commitmentApiClient.GetAllEmployerAccountIds().Result;
        }

        public List<User> GetUsersFromAccountId(long AccountId)
        {
            var accounts = _accountApiClient.GetAccountUsers(AccountId).Result;
            var Users = accounts
                .Where(acc => acc.CanReceiveNotifications)
                .Select(acc => MapTeamMemberToUser(acc,AccountId));
            return Users.ToList();
        }

        public List<Apprenticeship> GetApprenticeshipData()
        {
            var employerAccountIds = GetCommitmentEmployerIdsData();
            var commitApprenticeships = employerAccountIds.AsParallel()
                .SelectMany(id => _commitmentApiClient.GetEmployerApprenticeships(id).Result)
                .ToList();

            return commitApprenticeships;
        }

        public List<Apprenticeship> GetValidApprenticeshipCommitments(List<Apprenticeship> apprenticeships)
        {
            var validApps = apprenticeships
                .Where(app => app != null)
                .Where(app => app.HasHadDataLockSuccess == true)
                .Where(app => app.PaymentStatus == PaymentStatus.Active || app.PaymentStatus == PaymentStatus.Paused)
                .GroupBy(ea => new { ea.EmployerAccountId, ea.ProviderId })
                .Select(group => group.First())
                .ToList();

            return validApps;
        }

        public User MapTeamMemberToUser(TeamMemberViewModel tmvw,long AccountId)
        {
            return new User
            {
                EmailAddress = tmvw.Email,
                UserRef = Guid.Parse(tmvw.UserRef),
                AccountId = AccountId,
                FirstName = tmvw.Name.Split(' ')[0]
            };
        }

        public IEnumerable<DataRefreshMessage> Combine(Apprenticeship apprenticeship)
        {
            var users = GetUsersFromAccountId(apprenticeship.EmployerAccountId);

            var messages = users.Select(u => CreateMessageFromUserAndApprenticeship(u, apprenticeship));

            return messages;
        }

        public DataRefreshMessage CreateMessageFromUserAndApprenticeship(User user, Apprenticeship apprenticeship)
        {
            return new DataRefreshMessage
            {
                Provider = new Provider
                {
                    Ukprn = apprenticeship.ProviderId,
                    ProviderName = apprenticeship.ProviderName
                },
                User = user
            };
        }
    }
}
