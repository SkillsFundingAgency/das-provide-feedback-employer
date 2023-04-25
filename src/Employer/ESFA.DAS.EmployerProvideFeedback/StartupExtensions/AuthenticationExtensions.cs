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
using SFA.DAS.GovUK.Auth.Services;

namespace ESFA.DAS.EmployerProvideFeedback.StartupExtensions
{
    public static class AuthenticationExtensions
    {
        public static void AddEmployerAuthentication(this IServiceCollection services, ProvideFeedbackEmployerWebConfiguration provideFeedbackEmployerWebConfiguration, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddTransient<ICustomClaims, EmployerAccountPostAuthenticationClaimsHandler>();
            services.AddTransient<IEmployerAccountAuthorisationHandler, EmployerAccountAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, EmployerAccountAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, EmployerViewerTransactorAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, AccountActiveAuthorizationHandler>();//TODO remove after gov login go live
            
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
                        policy.RequireClaim(EmployerClaims.Account);
                        policy.Requirements.Add(new EmployerAccountRequirement());
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new AccountActiveRequirement());
                    });
                options.AddPolicy(
                    PolicyNames.HasEmployerViewerTransactorAccount
                    , policy =>
                    {
                        policy.RequireClaim(EmployerClaims.Account);
                        policy.Requirements.Add(new EmployerViewerTransactorRoleRequirement());
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new AccountActiveRequirement());
                    });
            });

            if (provideFeedbackEmployerWebConfiguration.UseGovSignIn)
            {
                services.Configure<GovUkOidcConfiguration>(configuration.GetSection("GovUkOidcConfiguration"));
                services.AddAndConfigureGovUkAuthentication(configuration, typeof(EmployerAccountPostAuthenticationClaimsHandler), "", "/SignIn-Stub");
            }
            else
            {
                var authenticationConfiguration = provideFeedbackEmployerWebConfiguration.Authentication;
                
                services
                .AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    sharedOptions.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;

                }).AddOpenIdConnect(options =>
                {
                    options.ClientId = authenticationConfiguration.ClientId;
                    options.ClientSecret = authenticationConfiguration.ClientSecret;
                    options.Authority = authenticationConfiguration.BaseAddress;
                    options.UsePkce = authenticationConfiguration.UsePkce;
                    options.ResponseType = authenticationConfiguration.ResponseType;

                    var scopes = authenticationConfiguration.Scopes.Split(' ');

                    foreach (var scope in scopes)
                    {
                        options.Scope.Add(scope);
                    }
                    options.ClaimActions.MapUniqueJsonKey("sub", "id");
                    options.Events.OnRemoteFailure = c =>
                    {
                        if (c.Failure.Message.Contains("Correlation failed"))
                        {
                            c.Response.Redirect("/");
                            c.HandleResponse();
                        }

                        return Task.CompletedTask;
                    };
                })
                .AddCookie(options =>
                {
                    options.AccessDeniedPath = new PathString("/error/403");
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.Cookie.Name = "SFA.DAS.ProvideFeedbackEmployer.Web.Auth";
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.SlidingExpiration = true;
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.CookieManager = new ChunkingCookieManager() { ChunkSize = 3000 };
                });
                services
                    .AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
                    .Configure<ICustomClaims>((options, customClaims) =>
                    {
                        options.Events.OnTokenValidated = async (ctx) =>
                        {
                            var claims = await customClaims.GetClaims(ctx);
                            ctx.Principal.Identities.First().AddClaims(claims);
                        };
                    });
            }
            
        }
    }
}
