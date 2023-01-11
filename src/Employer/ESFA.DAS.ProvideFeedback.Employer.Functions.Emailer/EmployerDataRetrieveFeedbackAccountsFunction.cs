using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class EmployerDataRetrieveFeedbackAccountsFunction
    {
        private readonly ICommitmentService _commitmentService;

        public EmployerDataRetrieveFeedbackAccountsFunction(ICommitmentService commitmentService)
        {
            _commitmentService = commitmentService;
        }

        [FunctionName("EmployerDataRetrieveFeedbackAccountsFunction")]
        public async Task Run(
            [ServiceBusTrigger("%RetrieveFeedbackAccountsQueueName%", Connection = "ServiceBusConnection")] string myQueueItem,
            [ServiceBus("%AccountRefreshQueueName%", Connection = "ServiceBusConnection", EntityType = EntityType.Queue)] ICollector<string> queue,
            ILogger log)
        {
            log.LogInformation($"Starting Employer Account retrieval");

            try
            {
                var allCohortAccountIdsResponse = await _commitmentService.GetAllCohortAccountIds();

                log.LogInformation("Finished getting the Employer Accounts from APIs");

                allCohortAccountIdsResponse
                    .AccountIds
                    .Select(id => id.ToString())
                    .AsParallel()
                    .ForAll(queue.Add);

                log.LogInformation($"Placed {allCohortAccountIdsResponse.AccountIds.Count()} messages in the queue");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Failed to retrieve Employer Accounts.");
                throw;
            }
        }
    }
}
