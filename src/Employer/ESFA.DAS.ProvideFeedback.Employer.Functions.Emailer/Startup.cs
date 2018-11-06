using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http.Headers;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.DependencyInjection.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
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
        private readonly IConfigurationRoot _configuration;
        public Startup()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public void Configure(IWebJobsBuilder builder) =>
            builder.AddDependencyInjection(ConfigureServices);

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddSingleton<IDbConnection>(c => new SqlConnection(_configuration.GetConnectionString("EmployerEmailStoreConnection")));
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
                //c.BaseAddress =  new Uri(notificationApiConfig.BaseUrl);
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", notificationApiConfig.ClientToken);
            });

            //services.AddTransient<INotificationsApi, NotificationsApi>();
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
        }
    }
}
