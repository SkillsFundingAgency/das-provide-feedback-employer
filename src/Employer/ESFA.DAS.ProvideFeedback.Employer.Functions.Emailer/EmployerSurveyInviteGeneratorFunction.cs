using System;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

            await _surveyInviteGenerator.GenerateSurveyInvites(message);

            log.LogInformation($"Employer Survey Invite generator completed at: {DateTime.Now}");
        }
    }
}
