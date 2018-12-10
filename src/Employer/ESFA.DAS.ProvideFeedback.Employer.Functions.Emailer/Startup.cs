using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http.Headers;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.DependencyInjection;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.DependencyInjection.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Client.Configuration;

[assembly: WebJobsStartup(typeof(Startup))]
namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer
{
    internal class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddDependencyInjection<ServiceProviderBuilder>();
        }
    }

    internal class ServiceProviderBuilder : IServiceProviderBuilder
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;
        

        public ServiceProviderBuilder(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public IServiceProvider Build()
        {
            var services = new ServiceCollection();
            // services.AddSingleton<ILoggerFactory, LoggerFactory>();

            // Important: We need to call CreateFunctionUserCategory, otherwise our log entries might be filtered out.
            services.AddSingleton<ILogger>(_ => _loggerFactory.CreateLogger(LogCategories.CreateFunctionUserCategory("Common")));
            services.AddSingleton<IDbConnection>(c => new SqlConnection(_configuration.GetConnectionStringOrSetting("EmployerEmailStoreConnection")));
            services.AddLogging((options) =>
            {
                options.AddConfiguration(_configuration.GetSection("Logging"));
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddConsole();
                options.AddDebug();
            });

            services.Configure<EmailSettings>(_configuration.GetSection("EmailSettings"));

            var notificationApiConfig = _configuration.GetSection("NotificationApi").Get<NotificationApiConfig>();

            services.AddHttpClient<INotificationsApi, NotificationsApi>(c =>
            {
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", notificationApiConfig.ClientToken);
            });

            services.AddSingleton<INotificationsApiClientConfiguration, NotificationsApiClientConfiguration>(a =>
                new NotificationsApiClientConfiguration
                {
                    ApiBaseUrl = notificationApiConfig.BaseUrl,
                    ClientToken = notificationApiConfig.ClientToken
                }
            );

            services.AddSingleton<EmployerSurveyInviteEmailer>();
            services.AddSingleton<EmployerSurveyReminderEmailer>();
            services.AddSingleton<IStoreEmployerEmailDetails, EmployerEmailDetailRepository>();

            return services.BuildServiceProvider();
        }
    }
}
