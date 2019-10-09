using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using ESFA.DAS.ProvideFeedback.Employer.Application;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class AccountRefreshFunction
    {
        private readonly EmployerFeedbackDataRetrievalService _dataRetrievalService;

        public AccountRefreshFunction(EmployerFeedbackDataRetrievalService dataRetrievalService)
        {
            _dataRetrievalService = dataRetrievalService;
        }

        [FunctionName("AccountRefreshFunction")]
        public async Task Run(
            [ServiceBusTrigger("%AccountRefreshQueueName%", Connection = "ServiceBusConnection")]string accountIdMessage,
            [ServiceBus("%ProcessActiveFeedbackQueueName%", Connection = "ServiceBusConnection", EntityType = EntityType.Queue)]ICollector<EmployerFeedbackRefreshMessage> queue,
            ILogger log)
        {
            log.LogInformation($"Account refresh function triggered for: {accountIdMessage}");

            try
            {
                var success = long.TryParse(accountIdMessage, out long accountId);
                if (success)
                {
                    var refreshMessages = await _dataRetrievalService.GetRefreshData(accountId);

                    refreshMessages
                        .ToList()
                        .ForEach(queue.Add);
                }
                else
                {
                    log.LogWarning($"AccoundId: {accountIdMessage} is not a valid accountId");
                }
            }
            catch(Exception ex)
            {
                log.LogError(ex, $"Account refresh function failed for accountId: {accountIdMessage}");
                throw;
            }
        }
    }
}
