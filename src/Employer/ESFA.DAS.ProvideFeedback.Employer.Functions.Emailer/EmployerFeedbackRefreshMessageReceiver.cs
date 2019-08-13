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
    public class EmployerFeedbackRefreshMessageReceiver
    {
        private readonly DataRefreshHelper _helper;
        private readonly ILogger<EmployerFeedbackRefreshMessageReceiver> _logger;

        public EmployerFeedbackRefreshMessageReceiver(
            DataRefreshHelper helper,
            ILogger<EmployerFeedbackRefreshMessageReceiver> logger)
        {
            _helper = helper;
            _logger = logger;
        }

        [FunctionName("DataRefreshMessageReceiver")]
        public async Task Run(
            [ServiceBusTrigger("data-refresh-messages", Connection = "ServiceBusConnection")]string myQueueItem,
            ILogger log,
            [ServiceBus("generate-survey-invite", Connection = "ServiceBusConnection", EntityType = EntityType.Queue)]IAsyncCollector<GenerateSurveyCodeMessage> queue)
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
