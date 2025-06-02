using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Authentication;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using SFA.DAS.GovUK.Auth.AppStart;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.GovUK.Auth.Configuration;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;

namespace ESFA.DAS.EmployerProvideFeedback.StartupExtensions
{
    public static class AuthenticationExtensions
    {
        public static void AddEmployerAuthentication(this IServiceCollection services, ProvideFeedbackEmployerWebConfiguration provideFeedbackEmployerWebConfiguration, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddTransient<IEmployerAccountAuthorisationHandler, EmployerAccountAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, EmployerAccountAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, EmployerViewerTransactorAuthorizationHandler>();
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyNames.EmployerAuthenticated
                , policy =>
                {
                    policy.RequireClaim(EmployerClaims.EmailAddress);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new AccountActiveRequirement());
                });
                options.AddPolicy(
                    PolicyNames.HasEmployerAccount
                    , policy =>
                    {
                        policy.RequireClaim(EmployerClaims.AccountsClaimsTypeIdentifier);
                        policy.Requirements.Add(new EmployerAccountRequirement());
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new AccountActiveRequirement());
                    });
                options.AddPolicy(
                    PolicyNames.HasEmployerViewerTransactorAccount
                    , policy =>
                    {
                        policy.RequireClaim(EmployerClaims.AccountsClaimsTypeIdentifier);
                        policy.Requirements.Add(new EmployerViewerTransactorRoleRequirement());
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new AccountActiveRequirement());
                    });
            });

            services.Configure<GovUkOidcConfiguration>(configuration.GetSection("GovUkOidcConfiguration"));
            services.AddAndConfigureGovUkAuthentication(configuration, 
                new AuthRedirects
                {
                    SignedOutRedirectUrl = "",
                    LocalStubLoginPath = "/SignIn-Stub"
                },
                null,
                typeof(EmployerAccountService));
        }
    }
}
