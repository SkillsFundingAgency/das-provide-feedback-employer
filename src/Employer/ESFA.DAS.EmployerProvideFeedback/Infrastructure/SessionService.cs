using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ESFA.DAS.EmployerProvideFeedback.Infrastructure
{
    public class SessionService : ISessionService
    {
        private readonly IDistributedCache _sessionCache;
        private readonly string _environment;
        private readonly int _slidingExpirationMinutes;

        public SessionService(
            IDistributedCache sessionCache,
            ProvideFeedbackEmployerWebConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _slidingExpirationMinutes = configuration.SlidingExpirationMinutes;
            _sessionCache = sessionCache;
            _environment = environment.EnvironmentName;
        }

        public async Task<T> Get<T>(string key)
        {
            var sessionObject = await GetString(key);
            return string.IsNullOrWhiteSpace(sessionObject) ? default(T) : JsonConvert.DeserializeObject<T>(sessionObject); //adhoc and email journey
        }

        public async Task<string> GetString(string key)
        {
            return await _sessionCache.GetStringAsync(_environment + "_" + key); //adhoc and email journey
        }

        public async Task Set(string key, object value)
        {
            //adhoc and email journey
            await _sessionCache.SetStringAsync(_environment + "_" + key, JsonConvert.SerializeObject(value), new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(_slidingExpirationMinutes) //adhoc and email journeys
            });
        }

        public async Task Remove(string key)
        {
            //email journey
            await _sessionCache.RemoveAsync(_environment + "_" + key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await GetString(key) != null;
        }
    }
}
