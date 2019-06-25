using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Framework.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public static class EmployerFeedbackRefreshMessageReceiver
    {
        [FunctionName("DataRefreshMessageReceiver")]
        public static async Task Run(
            [ServiceBusTrigger("data-refresh-messages", Connection = "ServiceBusConnection")]string myQueueItem, 
            ILogger log, 
            [Inject] IStoreEmployerEmailDetails dbRepository)
        {
            EmployerFeedbackRefreshMessage message = JsonConvert.DeserializeObject<EmployerFeedbackRefreshMessage>(myQueueItem);
            try
            {
                log.LogInformation("Starting upserting users");
                await dbRepository.UpsertIntoUsers(message.User);
                log.LogInformation("Done upserting users\nStarting upserting providers");
                await dbRepository.UpsertIntoProvidersAsync(message.Provider);
                log.LogInformation("Done upserting providers\nStarting upserting feedback");
                await dbRepository.UpsertIntoFeedbackAsync(message.User, message.Provider);
                log.LogInformation("Done upserting feedback\nStarting code generation");
                await dbRepository.GetOrCreateSurveyCode(message.User.UserRef, message.Provider.Ukprn,message.User.AccountId);
                log.LogInformation("Done code generation\nCommiting transaction");
            }
            catch(Exception ex)
            {
                log.LogError(ex.Message);
                log.LogError(ex.StackTrace);
            }
        }
    }
}
