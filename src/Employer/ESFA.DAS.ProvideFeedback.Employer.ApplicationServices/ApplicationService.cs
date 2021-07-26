namespace ESFA.DAS.ProvideFeedback.Employer.ApplicationServices
{
    public class ApplicationService
    {
        protected string GetBaseUrl(string apiBaseUrl)
        {
            return apiBaseUrl.EndsWith("/")
                ? apiBaseUrl
                : apiBaseUrl + "/";
        }
    }
}
