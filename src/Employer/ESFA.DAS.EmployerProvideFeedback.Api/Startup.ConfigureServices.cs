using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Api.Common.AppStart;
using SFA.DAS.Api.Common.Configuration;
using SFA.DAS.Api.Common.Infrastructure;
using AutoMapper;
using ESFA.DAS.EmployerProvideFeedback.Api.Configuration;
using ESFA.DAS.EmployerProvideFeedback.Api.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace ESFA.DAS.EmployerProvideFeedback.Api
{
    
    /// <summary>
    /// The ConfigureServices method
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    public partial class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup (IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            
            var azureAdConfiguration = _configuration
                .GetSection("AzureAd")
                .Get<AzureActiveDirectoryConfiguration>();

            var policies = new Dictionary<string, string>
            {
                {PolicyNames.Default, "Default"},
                {"GetFeedback", "GetFeedback"}
            };

            services.AddAuthentication(azureAdConfiguration, policies);
            
            var cosmosOptions = _configuration
                .GetSection("Azure")
                .Get<AzureOptions>();
            
            services.AddSingleton<IEmployerFeedbackRepository>(
                (svc) => CosmosEmployerFeedbackRepository.Instance.ConnectTo(cosmosOptions.CosmosEndpoint)
                    .WithAuthKeyOrResourceToken(cosmosOptions.CosmosKey)
                    .UsingDatabase(cosmosOptions.DatabaseName).UsingCollection(cosmosOptions.EmployerFeedbackCollection));

            services.Configure<AzureOptions>(_configuration.GetSection("Azure"));
            services.Configure<AzureAdOptions>(_configuration.GetSection("AzureAd"));
            services.AddMvc(options=>options.Filters.Add(new AuthorizeFilter("GetFeedback")));
            
            services.AddAutoMapper();
            services.AddControllers();
            
            services.AddSwaggerDocument();
            services.AddHealthChecks();

            
        }
    }
}