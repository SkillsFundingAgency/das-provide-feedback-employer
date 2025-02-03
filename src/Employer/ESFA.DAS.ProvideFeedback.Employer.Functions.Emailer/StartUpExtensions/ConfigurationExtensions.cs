using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Azure.Monitor.OpenTelemetry.AspNetCore;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.StartUpExtensions
{
    public static class ConfigurationExtensions
    {
        public static IConfiguration BuildDasConfiguration(this IConfigurationBuilder configBuilder, IConfiguration configuration)
        {
            configBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

#if DEBUG
            configBuilder.AddJsonFile("local.settings.json", optional: true);
#endif

            return configBuilder.Build();
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