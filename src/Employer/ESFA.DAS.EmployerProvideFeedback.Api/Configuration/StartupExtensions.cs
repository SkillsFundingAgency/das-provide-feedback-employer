using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Api.Common.Configuration;
using SFA.DAS.Api.Common.Infrastructure;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Api.Common.AppStart;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Azure.Services.AppAuthentication;
using Azure.Monitor.OpenTelemetry.AspNetCore;

namespace ESFA.DAS.EmployerProvideFeedback.Api.Configuration
{
    public static class StartupExtensions
    {
        public static void AddAuthentication(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            var azureAdConfiguration = configuration
                .GetSection("AzureAd")
                .Get<AzureActiveDirectoryConfiguration>();

            var policies = new Dictionary<string, string>
            {
                {PolicyNames.Default, "Default"},
                {"GetFeedback", "GetFeedback"}
            };

            services.Configure<AzureAdOptions>(configuration.GetSection("AzureAd"));

            if (env.IsDevelopment())
            {
                services.AddMvc();
            }
            else
            {
                services.AddAuthentication(azureAdConfiguration, policies);
                services.AddMvc(options => options.Filters.Add(new AuthorizeFilter("GetFeedback")));
            }
        }

        public static void AddDatabaseConnection(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            services.AddTransient<IDbConnection>(c =>
            {
                const string azureResource = "https://database.windows.net/";
                string connectionString = configuration.GetConnectionString("EmployerEmailStoreConnection");

                if (env.IsDevelopment())
                    return new SqlConnection(connectionString);

                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                return new SqlConnection
                {
                    ConnectionString = connectionString,
                    AccessToken = azureServiceTokenProvider.GetAccessTokenAsync(azureResource).Result
                };

            });
        }

        public static void AddOpenTelemetryRegistration(this IServiceCollection services, string appInsightsConnectionString)
        {
            if (!string.IsNullOrEmpty(appInsightsConnectionString))
            {
                // This service will collect and send telemetry data to Azure Monitor.
                services.AddOpenTelemetry().UseAzureMonitor(options =>
                {
                    options.ConnectionString = appInsightsConnectionString;
                });
            }
        }
    }
}
