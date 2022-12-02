using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;

namespace ESFA.DAS.ProvideFeedback.Employer.Application
{
    public class FeedbackSummariesService
    {
        private ILogger<FeedbackSummariesService> _logger;
        private IEmployerFeedbackRepository _dbRepository;
        private EmployerFeedbackSettings _settings;

        public FeedbackSummariesService(
            ILogger<FeedbackSummariesService> logger,
            IEmployerFeedbackRepository dbRepository,
            IOptions<EmployerFeedbackSettings> settings)
        {
            _logger = logger;
            _dbRepository = dbRepository;
            _settings = settings.Value;
        }

        public async Task GenerateFeedbackSummaries()
        {
            _logger.LogInformation("Starting GenerateFeedbackSummaries");
            bool toleranceParsedOk = decimal.TryParse(_settings.Tolerance, out decimal tolerance);
            Task<int> task1 = _dbRepository.GenerateProviderRatingResults(_settings.AllUsersFeedback, _settings.ResultsForAllTime, _settings.RecentFeedbackMonths, toleranceParsedOk ? tolerance : 0);
            Task<int> task2 = _dbRepository.GenerateProviderAttributeResults(_settings.AllUsersFeedback, _settings.ResultsForAllTime, _settings.RecentFeedbackMonths);
            await Task.WhenAll(task1, task2);
            _logger.LogInformation("GenerateFeedbackSummaries complete");
        }
    }
}
