﻿using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.Orchestrators;
using ESFA.DAS.EmployerProvideFeedback.Paging;
using ESFA.DAS.EmployerProvideFeedback.Services;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.Configuration;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.OuterApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Encoding;
using SFA.DAS.GovUK.Auth.Employer;
using SFA.DAS.GovUK.Auth.Services;

namespace ESFA.DAS.EmployerProvideFeedback.StartupExtensions
{
    public static class AddServiceRegistrationExtensions
    {
        public static void AddServiceRegistrations(this IServiceCollection services, ProvideFeedbackEmployerWebConfiguration configuration)
        {
            services.AddTransient<IEmployerFeedbackRepository, EmployerFeedbackRepository>();
            services.AddTransient<EnsureFeedbackNotSubmitted>();
            services.AddTransient<EnsureFeedbackNotSubmittedRecentlyAttribute>();
            services.AddTransient<EnsureSessionExists>();
            services.AddTransient<ISessionService, SessionService>();
            services.AddTransient<ReviewAnswersOrchestrator>();

            services.AddSingleton<ICommitmentApiConfiguration>(configuration.CommitmentsApiConfiguration);
            services.AddTransient<ICommitmentService, CommitmentService>();
            services.AddHttpClient<SecureHttpClient>();
            services.AddTransient<IAzureClientCredentialHelper, AzureClientCredentialHelper>();

            services.AddTransient<ITrainingProviderService, TrainingProviderService>();

            // Encoding Service
            services.AddSingleton<IEncodingService, EncodingService>();

            services.AddTransient<IGovAuthEmployerAccountService, EmployerAccountService>();
            services.AddHttpClient<IOuterApiClient, OuterApiClient>();

            services.AddTransient<IStubAuthenticationService, StubAuthenticationService>();//TODO remove one gov login is live
        }
    }
}
