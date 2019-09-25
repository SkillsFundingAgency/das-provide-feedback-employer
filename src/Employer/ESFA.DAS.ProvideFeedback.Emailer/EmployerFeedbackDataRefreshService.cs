using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Providers.Api.Client;
using Provider = ESFA.DAS.ProvideFeedback.Domain.Entities.Models.Provider;

namespace ESFA.DAS.Feedback.Employer.Emailer
{
    public class EmployerFeedbackDataRetrievalService
    {
        IProviderApiClient _providerApiClient;
        IEmployerCommitmentApi _commitmentApiClient;
        IAccountApiClient _accountApiClient;

        public EmployerFeedbackDataRetrievalService(IProviderApiClient providerApiClient, IEmployerCommitmentApi commitmentApiClient, IAccountApiClient accountApiClient)
        {
            _providerApiClient = providerApiClient;
            _commitmentApiClient = commitmentApiClient;
            _accountApiClient = accountApiClient;
        }

        public async Task<IEnumerable<EmployerFeedbackRefreshMessage>> GetRefreshData(long accountId)
        {
            var messages = new List<EmployerFeedbackRefreshMessage>();

                var teamMembers = await _accountApiClient.GetAccountUsers(accountId);
                var mappedUsers = teamMembers
                    .Where(au => au.CanReceiveNotifications)
                    .Select(au => MapTeamMemberToUser(au, accountId));

                var accCommitments = await _commitmentApiClient.GetEmployerApprenticeships(accountId);

                var providers = await _providerApiClient.FindAllAsync();

                var providersDictionary = providers
                    .Select(p => new Provider { ProviderName = p.ProviderName, Ukprn = p.Ukprn })
                    .ToDictionary(p2 => p2.Ukprn);

                var validCommitments = accCommitments
                .Where(app => app != null)
                .Where(app => app.HasHadDataLockSuccess == true)
                .Where(app => app.PaymentStatus == PaymentStatus.Active || app.PaymentStatus == PaymentStatus.Paused)
                .Where(app => providersDictionary.ContainsKey(app.ProviderId))
                .GroupBy(app => new { app.EmployerAccountId, app.ProviderId });

                foreach (var commitment in validCommitments)
                {
                    foreach (var user in mappedUsers)
                    {
                        messages.Add(new EmployerFeedbackRefreshMessage
                        {
                            Provider = providersDictionary[commitment.Key.ProviderId],
                            User = user
                        });
                    }
                }

            return messages;
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
