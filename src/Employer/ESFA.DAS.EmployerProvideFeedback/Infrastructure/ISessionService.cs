using System;
using System.Threading.Tasks;

namespace ESFA.DAS.EmployerProvideFeedback.Infrastructure
{
    public interface ISessionService
    {
        Task<T> Get<T>(string key);
        Task Set(string key, object value);
        Task Remove(string key);
        Task<bool> ExistsAsync(string key);
    }
}
