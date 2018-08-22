using System;
using ESFA.DAS.FeedbackDataAccess.Repositories;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ESFA.DAS.FeedbackDataAccess.IoC
{
    public static class ServiceCollectionExtensions
    {
        public static void AddProvideFeedbackCosmos(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IEmployerFeedbackRepository, EmployerFeedbackRepository>();
            services.AddTransient<IDocumentClient>(_ =>
            {
                var cosmosConfig = _.GetService<IOptions<CosmosConnectionSettings>>();
                return new DocumentClient(new Uri(cosmosConfig.Value.EndpointUrl), cosmosConfig.Value.AuthorizationKey);
            });
        }
    }
}
