using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Framework.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public static class DataRefreshMessageReceiver
    {
        [FunctionName("DataRefreshMessageReceiver")]
        public static void Run([ServiceBusTrigger("data-refresh-message", Connection = "ServiceBusConnection")]string myQueueItem, ILogger log, [Inject] EmployerFeedbackRepository dbRepository)
        {
            DataRefreshMessage message = JsonConvert.DeserializeObject<DataRefreshMessage>(myQueueItem);
            log.LogInformation("Starting upserting users");
            dbRepository.UpsertIntoUsers(message.User);
            log.LogInformation("Done upserting users\nStarting upserting providers");
            dbRepository.UpsertIntoProvidersAsync(message.Provider);
            log.LogInformation("Done upserting providers\nStarting upserting feedback");
            dbRepository.UpsertIntoFeedbackAsync(message.User, message.Provider);
            log.LogInformation("Done upserting feedback\nCommiting transaction");
        }
    }
}
