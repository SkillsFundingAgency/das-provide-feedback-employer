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

        public EmployerFeedbackRefreshDataFunction(DataRefreshHelper helper)
        {
            _helper = helper;
        }

        [FunctionName("EmployerFeedbackRefreshDataFunction")]
        public async Task Run(
            [ServiceBusTrigger("%DataRefreshMessagesQueueName%", Connection = "ServiceBusConnection")]string myQueueItem,
            ILogger log,
            [ServiceBus("%GenerateSurveyInviteMessageQueueName%", Connection = "ServiceBusConnection", EntityType = EntityType.Queue)]IAsyncCollector<GenerateSurveyCodeMessage> queue)
        {
            log.LogInformation("Data refresh function started.");
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
                log.LogError(ex, "Error refreshing feedback data");
                throw;
            }

            log.LogInformation("Data refresh function complete.");
        }

        
    }
}
