using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Employer.Shared.UI.Configuration;
using SFA.DAS.EmployerUrlHelper;
using SFA.DAS.EmployerUrlHelper.Configuration;
using SFA.DAS.EmployerUrlHelper.DependencyResolution;

namespace ESFA.DAS.EmployerProvideFeedback.StartupExtensions
{
    public static class AddEmployerSharedUIExtensions
    {
        public static void AddEmployerSharedUI(this IServiceCollection services, Authentication config, IConfiguration configuration)
        {
            services.AddEmployerUrlHelper(configuration);
            services.AddMaMenuConfiguration(configuration, "signout", config.ClientId);

            services.Configure<EmployerUrlHelperConfiguration>(configuration.GetSection("EmployerUrlHelper"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerUrlHelperConfiguration>>().Value);

            services.Configure<MaPageConfiguration>(configuration.GetSection("MaPageConfiguration"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<MaPageConfiguration>>().Value);
            services.PostConfigure<MaPageConfiguration>((options => options.LocalLogoutRouteName = "signout"));

            services.AddSingleton<ICookieBannerViewModel>(provider =>
            {
                var maLinkGenerator = provider.GetService<ILinkGenerator>();
                return new CookieBannerViewModel
                {
                    CookieDetailsUrl = maLinkGenerator.AccountsLink("cookieConsent/details"),
                    CookieConsentUrl = maLinkGenerator.AccountsLink("cookieConsent"),

                };
            });
        }
    }
}
