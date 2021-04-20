using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;

namespace ESFA.DAS.EmployerProvideFeedback.Api
{
    using AutoMapper;

    using Configuration;
    using Repository;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

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
            services.AddAutoMapper();

            services.AddControllers();

            var azureAdConfiguration = _configuration
                .GetSection("AzureAd")
                .Get<AzureAdOptions>();
            
            services.AddAuthentication(auth => { auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; })
                .AddJwtBearer(auth =>
                {
                    auth.Authority = $"https://login.microsoftonline.com/{azureAdConfiguration.Tenant}";
                    auth.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAudiences = azureAdConfiguration.Identifier.Split(',')
                    };
                });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("GetFeedback", (Action<AuthorizationPolicyBuilder>) (policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("GetFeedback");
                }));
            });
            
            
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
            services.AddSwaggerDocument();
            services.AddHealthChecks();

            
        }
    }
}