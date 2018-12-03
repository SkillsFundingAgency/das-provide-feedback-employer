using System;
using System.Threading.Tasks;

namespace ESFA.DAS.EmployerProvideFeedback.Infrastructure
{
    public interface ISessionService
    {
        Task<T> GetAsync<T>(string key);
        Task SetAsync(string key, object value);
        Task<bool> ExistsAsync(string key);
        void Set(string key, object value);
        void Set(string key, string stringValue);
        void Remove(string key);
        string Get(string key);
        T Get<T>(string key);
        bool Exists(string key);
    }
}
