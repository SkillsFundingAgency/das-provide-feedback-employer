using System.Threading.Tasks;
using SFA.DAS.Apprenticeships.Api.Types.Providers;

namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices
{
    public interface IGetProviderFeedback
    {
        Task<Feedback> GetProviderFeedbackAsync(long ukPrn);
    }
}
