using System;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
            [ServiceBusTrigger("generate-survey-invite", Connection = "ServiceBusConnection")]string feedbackForCodeGeneration,
            ILogger log)
        {
            log.LogInformation($"Employer Survey Invite generator executed at: {DateTime.Now}");
            var message = JsonConvert.DeserializeObject<GenerateSurveyCodeMessage>(feedbackForCodeGeneration);

            try
            {
                await _surveyInviteGenerator.GenerateSurveyInvites(message);
            }
            catch(Exception ex)
            {
                log.LogError(ex, $"Failed to generate survey invite for Account: {message.AccountId}, Ukprn: {message.Ukprn}, User: {message.UserRef}");
                throw;
            }

            log.LogInformation($"Employer Survey Invite generator completed at: {DateTime.Now}");
        }
    }
}
