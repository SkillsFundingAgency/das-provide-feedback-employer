using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Employer.Application;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.Configuration;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Options;
using System.Configuration;
using System.IO;
using System.Net.Http.Headers;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.Database;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.StartUpExtensions;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((hostBuilderContext, builder) => { builder.BuildDasConfiguration(hostBuilderContext.Configuration); })
    .ConfigureServices((context, s) =>
    {
        var serviceProvider = s.BuildServiceProvider();
        var configuration = context.Configuration;
        var configBuilder = new ConfigurationBuilder()
        .AddConfiguration(configuration)
        .SetBasePath(Directory.GetCurrentDirectory())
#if DEBUG
        .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
#endif
        .AddEnvironmentVariables();
        var config = configBuilder.Build();

        s.AddDatabaseRegistration(config);
        s.AddApplicationInsightsTelemetry();

        s.AddLogging(options =>
        {
            options.AddApplicationInsights();
            options.AddFilter<ApplicationInsightsLoggerProvider>("ESFA.DAS", Microsoft.Extensions.Logging.LogLevel.Information);
            options.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning);
        });

        s.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        s.Configure<EmployerFeedbackSettings>(config.GetSection("EmployerFeedbackSettings"));

        s.AddTransient<IEmployerFeedbackRepository, EmployerFeedbackRepository>();

        s.AddTransient<FeedbackSummariesService>();

        s.AddHttpClient<SecureHttpClient>();
        var commitmentV2ApiConfig = config.GetSection("CommitmentV2Api").Get<CommitmentApiConfiguration>();
        s.AddSingleton<ICommitmentApiConfiguration>(commitmentV2ApiConfig);
        s.AddSingleton<ICommitmentService, CommitmentService>();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddFilter("Microsoft", LogLevel.Warning);
    })
    .Build();

host.Run();