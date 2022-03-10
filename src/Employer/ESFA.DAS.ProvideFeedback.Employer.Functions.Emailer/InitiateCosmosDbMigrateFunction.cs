using ESFA.DAS.EmployerProvideFeedback.Api.Dto;
using ESFA.DAS.EmployerProvideFeedback.Api.Repository;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class InitiateCosmosDbMigrateFunction
    {
        private readonly IEmployerFeedbackRepository _employerFeedbackRepository;

        public InitiateCosmosDbMigrateFunction(IEmployerFeedbackRepository employerFeedbackRepository)
        {
           _employerFeedbackRepository = employerFeedbackRepository;
        }

        [FunctionName("InitiateCosmosDbMigrateFunction")]
        public async Task Run(
            [HttpTrigger(AuthorizationLevel.Function, "get","post", Route = null)] 
            HttpRequest req, ILogger log,
            [ServiceBus("%MigrateCosmosBatchRecordsQueueName%", Connection = "ServiceBusConnection", EntityType = EntityType.Queue)] ICollector<CosmosBatchMessage> queue)
        {
            log.LogInformation("Starting Cosmos DB Migration"); 

            try
            {
                var count = _employerFeedbackRepository.GetCountOfCollection<EmployerFeedback>();
                var batchCount = 100;
                int batches = (count / batchCount) + 1;
                for(int i = 0;i<batches;i++)
                {
                    queue.Add(new CosmosBatchMessage { Take = batchCount, Skip = i*batchCount });
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unable to get cosmos batches to migrate.");
                throw;
            }
        }
    }
}