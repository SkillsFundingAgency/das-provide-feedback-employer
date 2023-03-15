using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ESFA.DAS.ProvideFeedback.Employer.Application.Configuration;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class GenerateFeedbackSummaries
    {
        private readonly FeedbackSummariesService _feedbackSummariesService;

        public GenerateFeedbackSummaries(FeedbackSummariesService feedbackSummariesService)
        {
            _feedbackSummariesService = feedbackSummariesService;
        }

        [FunctionName("GenerateFeedbackSummariesFunction")]
        public async Task Run([TimerTrigger("0 0 */3 * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation("Generate feedback summaries function started.");

            try
            {
                await _feedbackSummariesService.GenerateFeedbackSummaries();
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Error generating feedback summaries");
                throw;
            }

            log.LogInformation("Generate feedback summaries function complete.");
        }        
    }
}
