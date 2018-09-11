using System.Linq;
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

        public void Set(string key, object value)
        {
            _httpContextAccessor.HttpContext.Session.SetString(_environment + "_" + key,
                JsonConvert.SerializeObject(value));
        }

        public void Set(string key, string stringValue)
        {
            _httpContextAccessor.HttpContext.Session.SetString(_environment + "_" + key,
                stringValue);
        }

        public void Remove(string key)
        {
            _httpContextAccessor.HttpContext.Session.Remove(_environment + "_" + key);
        }

        public string Get(string key)
        {
            return _httpContextAccessor.HttpContext.Session.GetString(_environment + "_" + key);
        }

        public T Get<T>(string key)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            key = _environment + "_" + key;
            var sessionObject = string.Empty;

            if (KeyExists(session, key))
            {
                sessionObject = session.GetString(key);
            }

            return string.IsNullOrWhiteSpace(sessionObject) ? default(T) : JsonConvert.DeserializeObject<T>(sessionObject);
        }

        public bool Exists(string key)
        {
            key = _environment + "_" + key;
            var session = _httpContextAccessor.HttpContext.Session;
            return KeyExists(session, key);
        }

        private bool KeyExists(ISession session, string key)
        {
            return session.Keys.Any(k => k == key);
        }
    }
}
