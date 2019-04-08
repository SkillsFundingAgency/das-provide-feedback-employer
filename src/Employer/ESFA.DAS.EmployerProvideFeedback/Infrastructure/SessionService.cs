using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ESFA.DAS.EmployerProvideFeedback.Infrastructure
{
    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _environment;

        public SessionService(IHttpContextAccessor httpContextAccessor, IHostingEnvironment environment)
        {
            _httpContextAccessor = httpContextAccessor;
            _environment = environment.EnvironmentName;
        }

        public async Task<T> Get<T>(string key)
        {
            var session = _httpContextAccessor.HttpContext.Session;

            await session.LoadAsync();
            key = _environment + "_" + key;
            var sessionObject = string.Empty;

            if (KeyExists(session, key))
            {
                sessionObject = session.GetString(key);
            }

            return string.IsNullOrWhiteSpace(sessionObject) ? default(T) : JsonConvert.DeserializeObject<T>(sessionObject);
        }

        public async Task Set(string key, object value)
        {
            var session = _httpContextAccessor.HttpContext.Session;

            await session.LoadAsync();
            session.SetString(_environment + "_" + key, JsonConvert.SerializeObject(value));
            await session.CommitAsync();
        }

        public async Task<bool> Exists(string key)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            await session.LoadAsync();

            key = _environment + "_" + key;
            
            return KeyExists(session, key);
        }
       
        private bool KeyExists(ISession session, string key)
        {
            return session.Keys.Any(k => k == key);
        }
    }
}
