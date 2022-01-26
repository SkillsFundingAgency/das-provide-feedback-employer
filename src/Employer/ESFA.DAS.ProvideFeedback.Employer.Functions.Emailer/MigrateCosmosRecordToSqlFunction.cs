using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    public class MigrateCosmosRecordToSqlFunction
    {
        private readonly IEmployerFeedbackRepository _employerFeedbackRepository;

        public MigrateCosmosRecordToSqlFunction(IEmployerFeedbackRepository employerFeedbackRepository)
        {
            _employerFeedbackRepository = employerFeedbackRepository;
        }

        [FunctionName("MigrateCosmosRecordToSqlFunction")]
        public async Task Run(
            [ServiceBusTrigger("%MigrateCosmosRecordQueueName%", Connection = "ServiceBusConnection")] string cosmosRecord, ILogger log)
        {
            log.LogInformation("Adding record to SQL ");
            try
            {
                var cosmosFeedback = JsonConvert.DeserializeObject<CosmosEmployerFeedbackMessage>(cosmosRecord);
                
                if (cosmosFeedback == null)
                {
                    var message = $"Unable to add feedback record to SQL database, unable to deserialize {cosmosRecord}";
                    log.LogWarning(message);
                    return;
                }

                var feedbackRecord = await _employerFeedbackRepository.GetEmployerFeedbackRecord(cosmosFeedback.UserRef, cosmosFeedback.AccountId, cosmosFeedback.Ukprn);
                if (feedbackRecord == null)
                {
                    var message = $"Skipping message: Unable to find EmployerFeedback record for Cosmos Record Id:{cosmosFeedback.Id}, AccountId:{cosmosFeedback.AccountId}, Ukprn:{cosmosFeedback.AccountId}, UserRef:{cosmosFeedback.UserRef}";
                    log.LogWarning(message);
                    return;
                }

                var feedbackResult = await _employerFeedbackRepository.GetEmployerFeedbackResultRecord(feedbackRecord.FeedbackId, cosmosFeedback.DateTimeCompleted);
                if(feedbackResult != null)
                {
                    log.LogWarning($"Feedback Result already recorded for FeedbackId:{feedbackResult.FeedbackId} and Date Completed:{cosmosFeedback.DateTimeCompleted}");
                    return;
                }

                var providerAttributeAnswers = await ConvertCosmosFeedbackAnswersToProviderAttributes(cosmosFeedback.FeedbackAnswers);
                if(!providerAttributeAnswers.Any())
                {
                    log.LogWarning($"Skipping Feedback. No valid Provider Attribute answers that match current questions.");
                    return;
                }

                await _employerFeedbackRepository.CreateEmployerFeedbackResult(feedbackRecord.FeedbackId, cosmosFeedback.ProviderRating, cosmosFeedback.DateTimeCompleted, providerAttributeAnswers);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unable to add feedback record to SQL database");
                throw;
            }
        }

        private async Task<IEnumerable<ProviderAttribute>> ConvertCosmosFeedbackAnswersToProviderAttributes(IEnumerable<FeedbackAnswer> attributes)
        {
            var feedbackAttributes = await _employerFeedbackRepository.GetAllAttributes();
            var providerAttributes = new List<ProviderAttribute>();

            foreach (var attribute in attributes.Where(s => s.Value != 0))
            {
                var feedbackAnswer = feedbackAttributes.FirstOrDefault(s => s.AttributeName.Equals(attribute.Name.Trim(), StringComparison.InvariantCultureIgnoreCase));
                if (feedbackAnswer != null && !providerAttributes.Any(s => s.AttributeId == feedbackAnswer.AttributeId))
                {
                    providerAttributes.Add(new ProviderAttribute
                    {
                        AttributeId = feedbackAnswer.AttributeId,
                        AttributeValue = attribute.Value,
                    });
                }
            }

            return providerAttributes;
        }
    }
}
