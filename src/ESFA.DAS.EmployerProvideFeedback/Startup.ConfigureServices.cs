using ESFA.DAS.EmployerProvideFeedback.Configuration;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ESFA.DAS.EmployerProvideFeedback
{
    public partial class Startup
    {
        private readonly AuthenticationConfiguration _authConfig;
        private readonly IHostingEnvironment _hostingEnvironment;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            _authConfig = Configuration.GetSection("Authentication").Get<AuthenticationConfiguration>();
            _hostingEnvironment = hostingEnvironment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ExternalLinksConfiguration>(Configuration.GetSection("ExternalLinks"));
            services.Configure<ManageApprenticeshipsRoutes>(Configuration.GetSection("ManageApprenticeshipsRoutes"));
            services.AddSingleton<ManageApprenticeshipsLinkHelper>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                             .RequireAuthenticatedUser()
                             .Build();

                options.Filters.Add(new AuthorizeFilter(policy));

            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddAuthenticationService(_authConfig, _hostingEnvironment);
        }
    }
}
