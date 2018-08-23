using System.Threading.Tasks;
using ESFA.DAS.FeedbackDataAccess.Models;
using Microsoft.Azure.Documents;

namespace ESFA.DAS.FeedbackDataAccess.Repositories
{
    public interface IEmployerFeedbackRepository
    {
        Task<Document> CreateItemAsync(EmployerFeedback item);
    }
}
