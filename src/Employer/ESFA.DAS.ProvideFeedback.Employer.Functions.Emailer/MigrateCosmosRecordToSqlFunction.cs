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

                if(cosmosFeedback.UserRef == Guid.Empty)
                {
                    var message = $"Unable to add feedback record to SQL database, No UserRef Associated to Cosmos Record {cosmosRecord}";
                    log.LogWarning(message);
                    return;
                }

                var feedbackRecord = await _employerFeedbackRepository.GetEmployerFeedbackRecord(cosmosFeedback.UserRef, cosmosFeedback.AccountId, cosmosFeedback.Ukprn);
                long feedbackId;
                if (feedbackRecord == null)
                {
                    var provider = await _employerFeedbackRepository.GetProviderByUkprn(cosmosFeedback.Ukprn);
                    if(provider == null)
                    {
                        var feedbackErrorMessage = $"Skipping: Cannot create EmployerFeedback Record as UKPRN for Provider does not exist:{cosmosFeedback.Id}, AccountId:{cosmosFeedback.AccountId}, Ukprn:{cosmosFeedback.Ukprn}, UserRef:{cosmosFeedback.UserRef}";
                        log.LogError(feedbackErrorMessage);
                        return;
                    }

                    var user = await _employerFeedbackRepository.GetUserByUserRef(cosmosFeedback.UserRef);
                    if (user== null)
                    {
                        var feedbackErrorMessage = $"Skipping: Cannot create EmployerFeedback Record as User Ref for user does not exist:{cosmosFeedback.Id}, AccountId:{cosmosFeedback.AccountId}, Ukprn:{cosmosFeedback.Ukprn}, UserRef:{cosmosFeedback.UserRef}";
                        log.LogError(feedbackErrorMessage);
                        return;
                    }

                    var message = $"Creating EmployerFeedback record for Cosmos Record Id:{cosmosFeedback.Id}, AccountId:{cosmosFeedback.AccountId}, Ukprn:{cosmosFeedback.Ukprn}, UserRef:{cosmosFeedback.UserRef}";
                    feedbackId = await _employerFeedbackRepository.UpsertIntoFeedback(cosmosFeedback.UserRef, cosmosFeedback.AccountId, cosmosFeedback.Ukprn);
                    log.LogInformation(message);
                }
                else
                {
                    feedbackId = feedbackRecord.FeedbackId;
                }

                var feedbackResult = await _employerFeedbackRepository.GetEmployerFeedbackResultRecord(feedbackId, cosmosFeedback.DateTimeCompleted);
                if (feedbackResult != null)
                {
                    log.LogWarning($"Feedback Result already recorded for FeedbackId:{feedbackId} and Date Completed:{cosmosFeedback.DateTimeCompleted}");
                    return;
                }

                var providerAttributeAnswers = await ConvertCosmosFeedbackAnswersToProviderAttributes(cosmosFeedback.FeedbackAnswers);
                if (!providerAttributeAnswers.Any())
                {
                    log.LogWarning($"Skipping Feedback. No valid Provider Attribute answers that match current questions. FeedbackId:{feedbackId}, AccountId:{cosmosFeedback.AccountId}, Ukprn:{cosmosFeedback.Ukprn}, UserRef:{cosmosFeedback.UserRef}");
                    return;
                }

                await _employerFeedbackRepository.CreateEmployerFeedbackResult(feedbackId, cosmosFeedback.ProviderRating, cosmosFeedback.DateTimeCompleted, providerAttributeAnswers);
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
