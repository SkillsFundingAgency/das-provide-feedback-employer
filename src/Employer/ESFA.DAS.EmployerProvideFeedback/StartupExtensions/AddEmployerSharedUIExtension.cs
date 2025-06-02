using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Employer.Shared.UI;

namespace ESFA.DAS.EmployerProvideFeedback.StartupExtensions
{
    public static class AddEmployerSharedUIExtensions
    {
        public static void AddEmployerSharedUI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMaMenuConfiguration( "signout", configuration["ResourceEnvironmentName"]);
            
            services.AddSingleton<ICookieBannerViewModel>(provider =>
            {
                var maLinkGenerator = provider.GetService<UrlBuilder>();
                return new CookieBannerViewModel
                {
                    CookieDetailsUrl = maLinkGenerator.AccountsLink("Cookies") + "/details",
                    CookieConsentUrl = maLinkGenerator.AccountsLink("Cookies"),

                };
            });
        }
    }
}
