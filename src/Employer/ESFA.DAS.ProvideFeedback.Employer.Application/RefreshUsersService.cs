using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.ProvideFeedback.Employer.Application
{
    public class UserRefreshService
    {
        private ILogger<UserRefreshService> _logger;
        private IStoreEmployerEmailDetails _dbRepository;

        public UserRefreshService(
            ILogger<UserRefreshService> logger,
            IStoreEmployerEmailDetails dbRepository)
        {
            _logger = logger;
            _dbRepository = dbRepository;
        }

        public async Task UpdateAccountUsers(GroupedFeedbackRefreshMessage message)
        {
            _logger.LogInformation("Starting upserting user");

            var userToUpsert = message.RefreshMessages
                .GroupBy(u => u.User.UserRef)
                .Select(FirstUser);

            await _dbRepository.UpsertIntoUsers(userToUpsert);

            _logger.LogInformation("Done upserting user");
        }

        private User FirstUser(IGrouping<Guid, EmployerFeedbackRefreshMessage> feedbackGrouping) => feedbackGrouping.First().User;
    }
}
