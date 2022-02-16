using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Encoding;

namespace ESFA.DAS.EmployerProvideFeedback.StartupExtensions
{
    public static class AddConfigurationOptionsExtension
    {
        public static void AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<ProvideFeedbackEmployerWeb>(configuration.GetSection("ProvideFeedbackEmployerWeb"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<ProvideFeedbackEmployerWeb>>().Value);

            services.Configure<Authentication>(configuration.GetSection("Authentication"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<Authentication>>().Value);

            services.Configure<EncodingConfig>(configuration.GetSection("EncodingService"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<EncodingConfig>>().Value);
        }
    }
}
