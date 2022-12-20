using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.OuterApi
{
    public interface IGetApiRequest 
    {
        [JsonIgnore]
        string GetUrl { get; }
    }
}