using System;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.ProvideFeedback.Domain.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class EmployerFeedbackRefreshMessageReceiver
    {
        private readonly DataRefreshMessageHelper _helper;

        public EmployerFeedbackRefreshMessageReceiver(DataRefreshMessageHelper helper)
        {
            _helper = helper;
        }

        [FunctionName("DataRefreshMessageReceiver")]
        public async Task Run(
            [ServiceBusTrigger("data-refresh-messages", Connection = "ServiceBusConnection")]string myQueueItem,
            ILogger log)
        {
            EmployerFeedbackRefreshMessage message = JsonConvert.DeserializeObject<EmployerFeedbackRefreshMessage>(myQueueItem);
            try
            {
                await _helper.SaveMessageToDatabase(message);
            }
            catch(Exception ex)
            {
                log.LogError(ex.Message);
                log.LogError(ex.StackTrace);
            }
        }

        
    }
}
