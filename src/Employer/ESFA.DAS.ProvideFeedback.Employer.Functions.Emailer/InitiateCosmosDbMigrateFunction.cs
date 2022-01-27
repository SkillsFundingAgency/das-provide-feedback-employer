using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Api.Dto;
using ESFA.DAS.EmployerProvideFeedback.Api.Repository;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;

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
            [ServiceBus("%MigrateCosmosRecordQueueName%", Connection = "ServiceBusConnection", EntityType = EntityType.Queue)] ICollector<CosmosEmployerFeedbackMessage> queue)
        {
            log.LogInformation("Starting Cosmos DB Migration"); 

            try
            {
                var allItems = await _employerFeedbackRepository.GetAllItemsAsync();
                var chunks = allItems.Chunk(100);

                foreach(var chunk in chunks)
                {
                    chunk.Select(s => new CosmosEmployerFeedbackMessage
                        {
                            Id = s.Id,
                            AccountId = s.AccountId,
                            Ukprn = s.Ukprn,
                            UserRef = s.UserRef,
                            FeedbackAnswers = s.ProviderAttributes.Select(t => new FeedbackAnswer { Name = t.Name, Value = t.Value }),
                            ProviderRating = s.ProviderRating,
                            DateTimeCompleted = s.DateTimeCompleted
                        }).AsParallel().ForAll(queue.Add);

                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unable to get cosmos records to migrate.");
                throw;
            }
        }
    }
}
