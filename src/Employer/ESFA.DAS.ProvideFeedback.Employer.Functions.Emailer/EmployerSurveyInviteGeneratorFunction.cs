using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using ESFA.DAS.ProvideFeedback.Employer.Application;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class EmployerSurveyInviteGeneratorFunction
    {
        private readonly SurveyInviteGenerator _surveyInviteGenerator;

        public EmployerSurveyInviteGeneratorFunction(SurveyInviteGenerator surveyInviteGenerator)
        {
            _surveyInviteGenerator = surveyInviteGenerator;
        }

        [FunctionName("EmployerSurveyInviteGenerator")]
        public async Task Run(
            [ServiceBusTrigger("%GenerateSurveyInviteMessageQueueName%", Connection = "ServiceBusConnection")] string feedbackForCodeGeneration,
            ILogger log)
        {
            log.LogInformation($"Employer Survey Invite generator executed at: {DateTime.UtcNow}");
            var message = JsonConvert.DeserializeObject<GenerateSurveyCodeMessage>(feedbackForCodeGeneration);

            try
            {
                await _surveyInviteGenerator.GenerateSurveyInvites(message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Failed to generate survey invite for Account: {message.AccountId}, Ukprn: {message.Ukprn}, User: {message.UserRef}");
                throw;
            }

            log.LogInformation($"Employer Survey Invite generator completed at: {DateTime.UtcNow}");
        }
    }
}
