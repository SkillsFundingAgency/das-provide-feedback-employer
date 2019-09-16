using System;
using System.Linq;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class EmployerDataRetrieverFunction
    {
        private readonly EmployerFeedbackDataRetrievalService _dataRetrievalService;

        public EmployerDataRetrieverFunction(
            EmployerFeedbackDataRetrievalService dataRetrievalService)
        {
            _dataRetrievalService = dataRetrievalService;
        }

        [FunctionName("EmployerDataRetrieverFunction")]
        public void Run(
            [ServiceBusTrigger("%RetrieveFeedbackDataMessageQueueName%", Connection = "ServiceBusConnection")]string myQueueItem,
            [ServiceBus("%DataRefreshMessagesQueueName%", Connection = "ServiceBusConnection", EntityType = EntityType.Queue)]ICollector<EmployerFeedbackRefreshMessage> queue,
            ILogger log)
        {
            log.LogInformation($"Starting Data retrieval");

            try
            {
                var result = _dataRetrievalService.GetRefreshData();

                log.LogInformation("Finished getting the data from APIs");

                result.AsParallel().ForAll(queue.Add);

                log.LogInformation($"Placed {result.Count} messages in the queue");
            }
            catch(Exception ex)
            {
                log.LogError(ex, $"Failed to retrieve feedback refresh data.");
                throw;
            }            
        }
    }
}
