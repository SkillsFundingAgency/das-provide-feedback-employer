using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.Orchestrators;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Encoding;

namespace ESFA.DAS.EmployerProvideFeedback.StartupExtensions
{
    public static class AddServiceRegistrationExtensions
    {
        public static void AddServiceRegistrations(this IServiceCollection services)
        {
            services.AddTransient<IEmployerFeedbackRepository, EmployerFeedbackRepository>();
            services.AddTransient<EnsureFeedbackNotSubmitted>();
            services.AddTransient<EnsureSessionExists>();
            services.AddTransient<ISessionService, SessionService>();
            services.AddTransient<ReviewAnswersOrchestrator>();

            // Encoding Service
            services.AddSingleton<IEncodingService, EncodingService>();
        }
    }
}
