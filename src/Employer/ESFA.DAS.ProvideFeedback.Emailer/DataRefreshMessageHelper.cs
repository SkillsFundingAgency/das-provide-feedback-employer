using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.Feedback.Employer.Emailer
{
    public class DataRefreshMessageHelper
    {
        private ILogger<DataRefreshMessageHelper> _logger;
        private IStoreEmployerEmailDetails _dbRepository;

        public DataRefreshMessageHelper(
            ILogger<DataRefreshMessageHelper> logger,
            IStoreEmployerEmailDetails dbRepository)
        {
            _logger = logger;
            _dbRepository = dbRepository;
        }

        public async Task SaveMessageToDatabase(EmployerFeedbackRefreshMessage message)
        {
            _logger.LogInformation("Starting upserting users");
            await _dbRepository.UpsertIntoUsers(message.User);
            _logger.LogInformation("Done upserting users\nStarting upserting providers");
            await _dbRepository.UpsertIntoProvidersAsync(message.Provider);
            _logger.LogInformation("Done upserting providers\nStarting upserting feedback");
            await _dbRepository.UpsertIntoFeedbackAsync(message.User, message.Provider);
            _logger.LogInformation("Done upserting feedback\nStarting code generation");
            await _dbRepository.GetOrCreateSurveyCode(message.User.UserRef, message.Provider.Ukprn, message.User.AccountId);
            _logger.LogInformation("Done code generation\nCommiting transaction");
        }
    }
}
