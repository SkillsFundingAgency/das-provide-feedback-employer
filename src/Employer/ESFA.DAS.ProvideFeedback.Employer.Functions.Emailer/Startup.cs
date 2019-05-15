using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http.Headers;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Framework.DependencyInjection.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using SFA.DAS.NLog.Targets.Redis.DotNetCore;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Client.Configuration;
using SFA.DAS.Http;
using LogLevel = NLog.LogLevel;
using SFA.DAS.Providers.Api.Client;
using SFA.DAS.Http.TokenGenerators;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Client;
using SFA.DAS.EAS.Account.Api.Client;

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
            services.AddTransient<IDbConnection>(c => new SqlConnection(_configuration.GetConnectionStringOrSetting("EmployerEmailStoreConnection")));

            services.AddLogging((options) =>
            {
                options.AddConfiguration(_configuration.GetSection("Logging"));
                options.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
                options.AddConsole();
                options.AddDebug();

                ConfigureNLog();
            });

            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

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
            services.AddTransient<IStoreEmployerEmailDetails, EmployerFeedbackRepository>();
            services.AddSingleton<EmployerFeedbackDataRefresh>();

            var providerApiConfig = _configuration.GetSection("ProviderApi").Get<ProviderApiConfig>();
            services.AddSingleton<IProviderApiClient, ProviderApiClient>(a =>
                 new ProviderApiClient(providerApiConfig.BaseUrl)
            );

            var commitmentApiConfig = _configuration.GetSection("CommitmentApi").Get<CommitmentsApiClientConfig>();
            var bearerToken = (IGenerateBearerToken)new JwtBearerTokenGenerator(commitmentApiConfig);
            var httpClient = new HttpClientBuilder().WithDefaultHeaders().WithBearerAuthorisationHeader(bearerToken).Build();
            services.AddSingleton<IEmployerCommitmentApi, EmployerCommitmentApi>(a =>
                 new EmployerCommitmentApi(httpClient, commitmentApiConfig)
            );

            var accApiConfig = _configuration.GetSection("AccountApi").Get<AccountApiConfiguration>();
            services.AddSingleton<IAccountApiClient, AccountApiClient>(a =>
                 new AccountApiClient(accApiConfig)
            );
        }

        private void ConfigureNLog()
        {
            var appName = _configuration.GetConnectionStringOrSetting("AppName");
            var localLogPath = _configuration.GetConnectionStringOrSetting("LogDir");
            var env = _configuration.GetConnectionStringOrSetting("ASPNETCORE_ENVIRONMENT");
            var config = new LoggingConfiguration();

            if (string.IsNullOrEmpty(env))
            {
                AddLocalTarget(config, localLogPath, appName);
            }
            else
            {
                AddRedisTarget(config, appName, env);
            }

            LogManager.Configuration = config;
        }

        private static void AddLocalTarget(LoggingConfiguration config, string localLogPath, string appName)
        {
            InternalLogger.LogFile = Path.Combine(localLogPath, $"{appName}\\nlog-internal.{appName}.log");
            var fileTarget = new FileTarget("Disk")
            {
                FileName = Path.Combine(localLogPath, $"{appName}\\{appName}.${{shortdate}}.log"),
                Layout = "${longdate} [${uppercase:${level}}] [${logger}] - ${message} ${onexception:${exception:format=tostring}}"
            };
            config.AddTarget(fileTarget);

            config.AddRule(GetMinLogLevel(), LogLevel.Fatal, "Disk");
        }

        private static void AddRedisTarget(LoggingConfiguration config, string appName, string environment)
        {
            var target = new RedisTarget
            {
                Name = "RedisLog",
                AppName = appName,
                EnvironmentKeyName = "ASPNETCORE_ENVIRONMENT",
                ConnectionStringName = "Redis",
                IncludeAllProperties = true,
                Layout = "${message}"
            };

            config.AddTarget(target);
            config.AddRule(GetMinLogLevel(), LogLevel.Fatal, "RedisLog");
        }

        private static LogLevel GetMinLogLevel() => LogLevel.FromString("Info");
    }
}
