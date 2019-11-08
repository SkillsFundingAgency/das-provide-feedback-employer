using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using ESFA.DAS.ProvideFeedback.Employer.Application;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class ProcessActiveFeedbackFunction
    {
        private readonly UserRefreshService _userRefreshService;

        public ProcessActiveFeedbackFunction(UserRefreshService userRefreshService)
        {
            _userRefreshService = userRefreshService;
        }

        [FunctionName("ProcessActiveFeedbackFunction")]
        public async Task Run(
            [ServiceBusTrigger("%ProcessActiveFeedbackQueueName%", Connection = "ServiceBusConnection")]string myQueueItem,
            ILogger log,
            [ServiceBus("%GenerateSurveyInviteMessageQueueName%", Connection = "ServiceBusConnection", EntityType = EntityType.Queue)]ICollector<GenerateSurveyCodeMessage> queue)
        {
            log.LogInformation("Data refresh function started.");
            GroupedFeedbackRefreshMessage message = JsonConvert.DeserializeObject<GroupedFeedbackRefreshMessage>(myQueueItem);

            try
            {
                await _userRefreshService.UpdateAccountUsers(message);

                message
                    .RefreshMessages
                    .Select(rm => new GenerateSurveyCodeMessage
                    {
                        UserRef = rm.User.UserRef,
                        AccountId = rm.User.AccountId,
                        Ukprn = rm.ProviderId
                    })
                    .AsParallel()
                    .ForAll(queue.Add);
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
