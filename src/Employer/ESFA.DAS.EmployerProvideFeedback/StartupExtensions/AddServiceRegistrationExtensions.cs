﻿using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.Orchestrators;
using ESFA.DAS.EmployerProvideFeedback.Paging;
using ESFA.DAS.EmployerProvideFeedback.Services;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Encoding;

namespace ESFA.DAS.EmployerProvideFeedback.StartupExtensions
{
    public static class AddServiceRegistrationExtensions
    {
        public static void AddServiceRegistrations(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IEmployerFeedbackRepository, EmployerFeedbackRepository>();
            services.AddTransient<EnsureFeedbackNotSubmittedRecently>();
            services.AddTransient<EnsureSessionExists>();
            services.AddTransient<ISessionService, SessionService>();
            services.AddTransient<ReviewAnswersOrchestrator>();

            var commitmentV2ApiConfig = configuration.GetSection("AccountApi").Get<CommitmentApiConfiguration>();
            services.AddSingleton<ICommitmentApiConfiguration>(commitmentV2ApiConfig);
            services.AddTransient<ICommitmentService, CommitmentService>();
            services.AddHttpClient<SecureHttpClient>();
            services.AddTransient<IAzureClientCredentialHelper, AzureClientCredentialHelper>();

            services.AddTransient<ITrainingProviderService, TrainingProviderService>();

            // Encoding Service
            services.AddSingleton<IEncodingService, EncodingService>();
        }
    }
}