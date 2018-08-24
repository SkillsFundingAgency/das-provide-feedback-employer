using System;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
using Esfa.Das.Feedback.Employer.Emailer.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Client.Configuration;

namespace Esfa.Das.Feedback.Employer.Emailer
{
    class Program
    {
        private static readonly string EnvironmentName;
        private static readonly bool IsDevelopment;

        static Program()
        {
            EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            IsDevelopment = EnvironmentName?.Equals("Development", StringComparison.CurrentCultureIgnoreCase) ?? false;
        }

        static void Main(string[] args)
        {
            ILogger logger = null;

            try
            {
                // .NET Core sets the source directory as the working directory - so change that:
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

                var configuration = BuildConfiguration();

                IServiceCollection serviceCollection = new ServiceCollection();
                var serviceProvider = ConfigureServices(serviceCollection, configuration).BuildServiceProvider();

                var factory = ConfigureLoggingFactory(serviceProvider, configuration);

                logger = factory.CreateLogger("Program");

                JobHostConfiguration jobConfiguration = GetHostConfiguration(serviceProvider, factory);
                var host = new JobHost(jobConfiguration);

                var cancellationToken = new WebJobsShutdownWatcher().Token;
                cancellationToken.Register(host.Stop);

                logger.LogInformation("Job Starting");

                host.RunAndBlock();

                logger.LogInformation("Job Stopping");
            }
            catch (Exception ex)
            {
                logger?.LogCritical(ex, "The Job has met with a horrible end!!");

                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        private static ILoggerFactory ConfigureLoggingFactory(ServiceProvider serviceProvider, IConfigurationRoot config)
        {
            var instrumentationKey = config["AppInsights_InstrumentationKey"];

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageProperties = true, CaptureMessageTemplates = true });
            NLog.LogManager.LoadConfiguration("nlog.config");

            //loggerFactory.AddApplicationInsights(instrumentationKey, null);

            return loggerFactory;
        }

        private static JobHostConfiguration GetHostConfiguration(ServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            // Host configuration
            var jobConfiguration = new JobHostConfiguration();
            jobConfiguration.JobActivator = new CustomJobActivator(serviceProvider);
            jobConfiguration.UseTimers();
            jobConfiguration.LoggerFactory = loggerFactory;

            if (IsDevelopment)
            {
                jobConfiguration.DashboardConnectionString = null; // Reduces errors in output.
                jobConfiguration.UseDevelopmentSettings();
            }

            return jobConfiguration;
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            // Setup configuration classes
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: false)
                .AddJsonFile($"appSettings.{EnvironmentName}.json", true)
                .AddEnvironmentVariables();

            if (IsDevelopment)
            {
                builder.AddUserSecrets<Program>();
            }

            var configuration = builder.Build();
            Environment.SetEnvironmentVariable("AzureWebJobsDashboard", configuration.GetConnectionString("WebJobsDashboard"));
            Environment.SetEnvironmentVariable("AzureWebJobsStorage", configuration.GetConnectionString("WebJobsStorage"));

            return configuration;
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Setup dependencies
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging((options) =>
            {
                options.AddConfiguration(configuration.GetSection("Logging"));
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddConsole();
                options.AddDebug();
            });

            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            var notificationApiConfig = configuration.GetSection("NotificationApi").Get<NotificationApiConfig>();

            services.AddHttpClient<INotificationsApi, NotificationsApi>(c =>
            {
                //c.BaseAddress =  new Uri(notificationApiConfig.BaseUrl);
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", notificationApiConfig.ClientToken);
            });

            //services.AddTransient<INotificationsApi, NotificationsApi>();
            services.AddTransient<INotificationsApiClientConfiguration, NotificationsApiClientConfiguration>(a =>
                new NotificationsApiClientConfiguration
                {
                    ApiBaseUrl = notificationApiConfig.BaseUrl,
                    ClientToken = notificationApiConfig.ClientToken
                }
            );

            services.AddTransient<EmployerEmailer>();
            services.AddTransient<IStoreEmailDetails, StubEmailDetailsStore>();

            // Add Jobs
            services.AddScoped<EmployerEmailerJob>();

            return services;
        }
    }
}
