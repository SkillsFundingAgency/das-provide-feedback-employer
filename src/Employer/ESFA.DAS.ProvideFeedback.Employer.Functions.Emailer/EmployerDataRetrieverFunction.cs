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
        private readonly ILogger<EmployerDataRetrieverFunction> _logger;

        public EmployerDataRetrieverFunction(
            EmployerFeedbackDataRetrievalService dataRetrievalService,
            ILogger<EmployerDataRetrieverFunction> logger)
        {
            _dataRetrievalService = dataRetrievalService;
            _logger = logger;
        }

        [FunctionName("EmployerDataRetrieverFunction")]
        public void Run(
            [ServiceBusTrigger("%RetrieveFeedbackDataMessageQueueName%", Connection = "ServiceBusConnection")]string myQueueItem,
            [ServiceBus("%DataRefreshMessagesQueueName%", Connection = "ServiceBusConnection", EntityType = EntityType.Queue)]ICollector<EmployerFeedbackRefreshMessage> queue,
            ILogger log)
        {
            _logger.LogInformation($"Starting Data retrieval");

            var result = _dataRetrievalService.GetRefreshData();

            _logger.LogInformation("Finished getting the data from APIs");

            result.AsParallel().ForAll(queue.Add);

            _logger.LogInformation($"Placed {result.Count} messages in the queue");
        }
    }
}
