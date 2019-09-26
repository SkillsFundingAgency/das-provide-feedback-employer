using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using SFA.DAS.Commitments.Api.Client.Interfaces;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class EmployerDataRetrieveFeedbackAccountsFunction
    {
        private readonly IEmployerCommitmentApi _commitmentApiClient;

        public EmployerDataRetrieveFeedbackAccountsFunction(IEmployerCommitmentApi commitmentApiClient)
        {
            _commitmentApiClient = commitmentApiClient;
        }

        [FunctionName("EmployerDataRetrieveFeedbackAccountsFunction")]
        public async Task Run(
            [ServiceBusTrigger("%RetrieveFeedbackAccountsQueueName%", Connection = "ServiceBusConnection")]string myQueueItem,
            [ServiceBus("%AccountRefreshQueueName%", Connection = "ServiceBusConnection", EntityType = EntityType.Queue)]ICollector<string> queue,
            ILogger log)
        {
            log.LogInformation($"Starting Employer Account retrieval");

            try
            {
                var employerAccountIds = await _commitmentApiClient.GetAllEmployerAccountIds();

                log.LogInformation("Finished getting the Employer Accounts from APIs");

                employerAccountIds
                    .Select(id => id.ToString())
                    .AsParallel()
                    .ForAll(queue.Add);

                log.LogInformation($"Placed {employerAccountIds.Count()} messages in the queue");
            }
            catch(Exception ex)
            {
                log.LogError(ex, $"Failed to retrieve Employer Accounts.");
                throw;
            }            
        }
    }
}
