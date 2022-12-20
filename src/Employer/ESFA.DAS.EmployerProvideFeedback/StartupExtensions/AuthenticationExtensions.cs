using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Authentication;

namespace ESFA.DAS.EmployerProvideFeedback.StartupExtensions
{
    public static class AuthenticationExtensions
    {
        public static void AddEmployerAuthentication(this IServiceCollection services, AuthenticationConfiguration configuration)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    PolicyNames.HasEmployerAccount
                    , policy =>
                    {
                        policy.RequireClaim(EmployerClaims.Account);
                        policy.Requirements.Add(new EmployerAccountRequirement());
                        policy.RequireAuthenticatedUser();
                    });
                options.AddPolicy(
                    PolicyNames.HasEmployerViewerTransactorAccount
                    , policy =>
                    {
                        policy.RequireClaim(EmployerClaims.Account);
                        policy.Requirements.Add(new EmployerViewerTransactorRoleRequirement());
                        policy.RequireAuthenticatedUser();
                    });
            });
            services
                .AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    sharedOptions.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;

                }).AddOpenIdConnect(options =>
                {
                    options.ClientId = configuration.ClientId;
                    options.ClientSecret = configuration.ClientSecret;
                    options.Authority = configuration.BaseAddress;
                    options.UsePkce = configuration.UsePkce;
                    options.ResponseType = configuration.ResponseType;

                    var scopes = configuration.Scopes.Split(' ');

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
        }
    }
}
