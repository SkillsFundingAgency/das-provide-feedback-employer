using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Employer.Application;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.Database;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using System.IO;

[assembly: FunctionsStartup(typeof(Startup))]
namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    internal class Startup : FunctionsStartup
    {
        private IConfigurationRoot _configuration;
        public override void Configure(IFunctionsHostBuilder builder)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddDatabaseRegistration(_configuration);

            builder.Services.AddApplicationInsightsTelemetry();

            builder.Services.AddLogging(options =>
            {
                options.AddApplicationInsights();
                options.AddFilter<ApplicationInsightsLoggerProvider>("ESFA.DAS", Microsoft.Extensions.Logging.LogLevel.Information);
                options.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning);
            });

            builder.Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            builder.Services.Configure<EmployerFeedbackSettings>(_configuration.GetSection("EmployerFeedbackSettings"));
            builder.Services.AddTransient<IEmployerFeedbackRepository, EmployerFeedbackRepository>();
            builder.Services.AddTransient<FeedbackSummariesService>();
        }
    }
}
