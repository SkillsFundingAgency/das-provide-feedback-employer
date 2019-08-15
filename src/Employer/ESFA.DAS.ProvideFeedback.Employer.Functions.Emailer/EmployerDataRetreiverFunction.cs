using System.Linq;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class EmployerDataRetreiverFunction
    {
        private readonly EmployerFeedbackDataRetrievalService _dataRetrievalService;
        private readonly ILogger<EmployerDataRetreiverFunction> _logger;

        public EmployerDataRetreiverFunction(
            EmployerFeedbackDataRetrievalService dataRetrievalService,
            ILogger<EmployerDataRetreiverFunction> logger)
        {
            _dataRetrievalService = dataRetrievalService;
            _logger = logger;
        }

        [FunctionName("EmployerDataRetreiverFunction")]
        public void Run(
            [ServiceBusTrigger("retrieve-feedback-data", Connection = "ServiceBusConnection")]string myQueueItem,
            [ServiceBus("data-refresh-messages", Connection = "ServiceBusConnection", EntityType = EntityType.Queue)]IAsyncCollector<EmployerFeedbackRefreshMessage> queue,
            ILogger log)
        {
            _logger.LogInformation($"Starting Data retrieval");

            var result = _dataRetrievalService.GetRefreshData();

            _logger.LogInformation("Finished getting the data from APIs");

            result.AsParallel().ForAll(x => queue.AddAsync(x));

            _logger.LogInformation($"Placed {result.Count} messages in the queue");
        }
    }
}
