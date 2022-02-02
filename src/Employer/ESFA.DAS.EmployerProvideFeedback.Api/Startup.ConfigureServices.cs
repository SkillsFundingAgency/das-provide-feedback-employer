using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Api.Common.AppStart;
using SFA.DAS.Api.Common.Configuration;
using SFA.DAS.Api.Common.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.Api.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

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
            services.Configure<AzureAdOptions>(_configuration.GetSection("AzureAd"));
            services.AddMvc(options=>options.Filters.Add(new AuthorizeFilter("GetFeedback")));

            services.AddMediatR(typeof(Startup));
            services.AddControllers();
            
            services.AddSwaggerDocument();
            services.AddHealthChecks();

            
        }
    }
}