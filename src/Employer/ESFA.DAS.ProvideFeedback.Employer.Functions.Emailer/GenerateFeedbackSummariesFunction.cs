
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ESFA.DAS.ProvideFeedback.Employer.Application;
using Microsoft.Azure.Functions.Worker;


namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class GenerateFeedbackSummaries
    {
        private readonly FeedbackSummariesService _feedbackSummariesService;
        private readonly ILogger<GenerateFeedbackSummaries> _logger;

        public GenerateFeedbackSummaries(FeedbackSummariesService feedbackSummariesService, ILogger<GenerateFeedbackSummaries> logger)
        {
            _feedbackSummariesService = feedbackSummariesService;
            _logger = logger;
        }

        [Function("GenerateFeedbackSummariesFunction")]
        public async Task Run([TimerTrigger("0 0 */3 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation("Generate feedback summaries function started.");

            try
            {
                await _feedbackSummariesService.GenerateFeedbackSummaries();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error generating feedback summaries");
                throw;
            }

            _logger.LogInformation("Generate feedback summaries function complete.");
        }        
    }
}
