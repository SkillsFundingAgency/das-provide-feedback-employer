using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Employer.Application;
using ESFA.DAS.ProvideFeedback.Employer.Application.Configuration;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.Configuration;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.Database;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using SFA.DAS.NLog.Targets.Redis.DotNetCore;
using System;
using System.IO;
using LogLevel = NLog.LogLevel;

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

            builder.Services.AddLogging((options) =>
            {

                options.AddConfiguration(_configuration.GetSection("Logging"));
                options.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });

                options.AddConsole();

                ConfigureNLog();
            });

            builder.Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            builder.Services.Configure<EmployerFeedbackSettings>(_configuration.GetSection("EmployerFeedbackSettings"));

            builder.Services.AddTransient<IEmployerFeedbackRepository, EmployerFeedbackRepository>();
            builder.Services.AddTransient<FeedbackSummariesService>();

            builder.Services.AddHttpClient<SecureHttpClient>();
            builder.Services.AddTransient<IAzureClientCredentialHelper, AzureClientCredentialHelper>();
        }

        private void ConfigureNLog()
        {
            var appName = _configuration.GetConnectionStringOrSetting("AppName");
            var localLogPath = _configuration.GetConnectionStringOrSetting("LogDir");
            var env = _configuration.GetConnectionStringOrSetting("ASPNETCORE_ENVIRONMENT");
            var config = new LoggingConfiguration();

            if (string.IsNullOrEmpty(env) || env.Equals("development", StringComparison.OrdinalIgnoreCase))
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
