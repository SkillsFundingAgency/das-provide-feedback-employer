using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Framework.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public static class EmployerFeedbackRefreshMessageReceiver
    {
        [FunctionName("DataRefreshMessageReceiver")]
        public static async Task Run(
            [ServiceBusTrigger("data-refresh-messages", Connection = "ServiceBusConnection")]string myQueueItem, 
            ILogger log, 
            [Inject] DataRefreshMessageHelper helper)
        {
            EmployerFeedbackRefreshMessage message = JsonConvert.DeserializeObject<EmployerFeedbackRefreshMessage>(myQueueItem);
            try
            {
                await helper.SaveMessageToDatabase(message);
            }
            catch(Exception ex)
            {
                log.LogError(ex.Message);
                log.LogError(ex.StackTrace);
            }
        }

        
    }
}
