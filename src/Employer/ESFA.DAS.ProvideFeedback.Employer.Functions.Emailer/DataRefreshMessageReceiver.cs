using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.Feedback.Employer.Emailer.Models;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Framework.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public static class DataRefreshMessageReceiver
    {
        [FunctionName("DataRefreshMessageReceiver")]
        public static void Run(
            [ServiceBusTrigger("data-refresh-message", Connection = "ServiceBusConnection")]string myQueueItem, 
            ILogger log, 
            [Inject] DbRepository dbRepository)
        {
            DataRefreshMessage message = JsonConvert.DeserializeObject<DataRefreshMessage>(myQueueItem);
            dbRepository.ReceiveDataRefreshMessage(message);
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        }
    }
}
