using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace ESFA.DAS.EmployerProvideFeedback.StartupExtensions
{
    public static class DataProtectionStartupExtensions
    {
        public static IServiceCollection AddDasDataProtection(this IServiceCollection services, ProvideFeedbackEmployerWebConfiguration webConfiguration, IWebHostEnvironment environment)
        {
            if (!environment.IsDevelopment())
            {
                var redisConnectionString = webConfiguration.RedisConnectionString;
                var dataProtectionKeysDatabase = webConfiguration.DataProtectionKeysDatabase;

                var redis = ConnectionMultiplexer.Connect($"{redisConnectionString},{dataProtectionKeysDatabase}");

                services.AddDataProtection()
                    .SetApplicationName("das-employer")
                    .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
            }
            return services;
        }
    }
}
