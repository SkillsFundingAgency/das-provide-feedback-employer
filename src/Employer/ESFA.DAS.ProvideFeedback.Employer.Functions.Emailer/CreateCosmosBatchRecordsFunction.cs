using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Api.Dto;
using ESFA.DAS.EmployerProvideFeedback.Api.Repository;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class CreateCosmosBatchRecordsFunction
    {
        private readonly IEmployerFeedbackRepository _employerFeedbackRepository;

        public CreateCosmosBatchRecordsFunction(IEmployerFeedbackRepository employerFeedbackRepository)
        {
            _employerFeedbackRepository = employerFeedbackRepository;
        }

        [FunctionName("CreateCosmosBatchRecordsFunction")]
        public async Task Run(
            [ServiceBusTrigger("%MigrateCosmosBatchRecordsQueueName%", Connection = "ServiceBusConnection")] string batchMessage, ILogger log,
            [ServiceBus("%MigrateCosmosRecordQueueName%", Connection = "ServiceBusConnection", EntityType = EntityType.Queue)] ICollector<CosmosEmployerFeedbackMessage> queue)
        {
            log.LogInformation("Getting batch records From Cosmos");
            try
            {
                var batch = JsonConvert.DeserializeObject<CosmosBatchMessage>(batchMessage);
                if (batch == null)
                {
                    var message = $"Unable to deserialize batch message {batchMessage}";
                    log.LogWarning(message);
                    return;
                }

                log.LogInformation($"Retrieving Batch,Skip:{batch.Skip}, Take:{batch.Take}");

                Expression<Func<EmployerFeedback, object>> orderByClause = x => x.DateTimeCompleted;
                Expression<Func<EmployerFeedback, bool>> predicate = x => true;
                var batchedFeedback = await _employerFeedbackRepository.GetOrderedItemsAsync(predicate, orderByClause, batch.Take, batch.Skip);

                batchedFeedback.Select(s => new CosmosEmployerFeedbackMessage
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
            catch (Exception ex)
            {
                log.LogError(ex, "Unable to add batched messages to queue.");
                throw;
            }
}
    }
}
