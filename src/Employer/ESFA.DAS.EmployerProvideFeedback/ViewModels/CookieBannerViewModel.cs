namespace ESFA.DAS.EmployerProvideFeedback.ViewModels
{
    public interface ICookieBannerViewModel
    {
        string CookieConsentUrl { get; }
        string CookieDetailsUrl { get; }
    }

    public class CookieBannerViewModel : ICookieBannerViewModel
    {
        public string CookieDetailsUrl { get; set; }
        public string CookieConsentUrl { get; set; }
    }
}
