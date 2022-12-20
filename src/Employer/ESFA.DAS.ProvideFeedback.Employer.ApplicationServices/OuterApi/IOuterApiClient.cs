using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.OuterApi
{
    public interface IOuterApiClient
    {
        Task<ApiResponse<TResponse>> Get<TResponse>(IGetApiRequest request);   
    }
}