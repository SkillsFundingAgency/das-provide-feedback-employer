using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

[assembly: FunctionsStartup(typeof(Startup))]
namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddApplicationInsightsTelemetry();

            builder.Services.AddLogging(options =>
            {
                options.AddApplicationInsights();
                options.AddFilter<ApplicationInsightsLoggerProvider>("ESFA.DAS", LogLevel.Information);
                options.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Warning);
            });
        }
    }
}
