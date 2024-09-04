using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Employer.Application;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.Configuration;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.Database;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Options;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Client.Configuration;
using System.IO;
using System.Net.Http.Headers;

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

            var notificationApiConfig = _configuration.GetSection("NotificationApi").Get<NotificationApiConfig>();

            builder.Services.AddHttpClient<INotificationsApi, NotificationsApi>(c =>
            {
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", notificationApiConfig.ClientToken);
            });

            builder.Services.AddSingleton<INotificationsApiClientConfiguration, NotificationsApiClientConfiguration>(a =>
                new NotificationsApiClientConfiguration
                {
                    ApiBaseUrl = notificationApiConfig.BaseUrl,
                    ClientToken = notificationApiConfig.ClientToken
                }
            );

            builder.Services.AddTransient<IEmployerFeedbackRepository, EmployerFeedbackRepository>();

            builder.Services.AddTransient<FeedbackSummariesService>();

            var accApiConfig = _configuration.GetSection("AccountApi").Get<AccountApiConfiguration>();
            builder.Services.AddSingleton<IAccountApiConfiguration>(accApiConfig);
            builder.Services.AddSingleton<IAccountService, AccountService>();
            builder.Services.AddHttpClient<SecureHttpClient>();

            builder.Services.Configure<RoatpApiConfiguration>(_configuration.GetSection("RoatpApi"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<RoatpApiConfiguration>>().Value);
            builder.Services.AddTransient<IAzureClientCredentialHelper, AzureClientCredentialHelper>();
            builder.Services.AddHttpClient<IRoatpService, RoatpService>();

            var commitmentV2ApiConfig = _configuration.GetSection("CommitmentV2Api").Get<CommitmentApiConfiguration>();
            builder.Services.AddSingleton<ICommitmentApiConfiguration>(commitmentV2ApiConfig);
            builder.Services.AddSingleton<ICommitmentService, CommitmentService>();
        }
    }
}
