using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ESFA.DAS.EmployerProvideFeedback.StartupExtensions
{
    public static class AddCacheExtensions
    {
        public static IServiceCollection AddCache(this IServiceCollection services, IHostEnvironment environment, ProvideFeedbackEmployerWebConfiguration config)
        {
            if (environment.IsDevelopment())
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddStackExchangeRedisCache(
                    options => { options.Configuration = config.RedisConnectionString; });
            }

            return services;
        }
    }
}
