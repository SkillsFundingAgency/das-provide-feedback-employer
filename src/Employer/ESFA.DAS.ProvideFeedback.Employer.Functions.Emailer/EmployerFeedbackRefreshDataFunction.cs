using System;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class EmployerFeedbackRefreshDataFunction
    {
        private readonly DataRefreshHelper _helper;
        private readonly ILogger<EmployerFeedbackRefreshDataFunction> _logger;

        public EmployerFeedbackRefreshDataFunction(
            DataRefreshHelper helper,
            ILogger<EmployerFeedbackRefreshDataFunction> logger)
        {
            _helper = helper;
            _logger = logger;
        }

        [FunctionName("DataRefreshMessageReceiver")]
        public async Task Run(
            [ServiceBusTrigger("%DataRefreshMessagesQueueName%", Connection = "ServiceBusConnection")]string myQueueItem,
            ILogger log,
            [ServiceBus("%GenerateSurveyInviteMessageQueueName%", Connection = "ServiceBusConnection", EntityType = EntityType.Queue)]IAsyncCollector<GenerateSurveyCodeMessage> queue)
        {
            _logger.LogInformation("Data refresh function started.");
            EmployerFeedbackRefreshMessage message = JsonConvert.DeserializeObject<EmployerFeedbackRefreshMessage>(myQueueItem);

            try
            {
                await _helper.RefreshFeedbackData(message);

                await queue.AddAsync(new GenerateSurveyCodeMessage
                {
                    UserRef = message.User.UserRef,
                    AccountId = message.User.AccountId,
                    Ukprn = message.Provider.Ukprn
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error refreshing feedback data");
            }

            _logger.LogInformation("Data refresh function complete.");
        }

        
    }
}