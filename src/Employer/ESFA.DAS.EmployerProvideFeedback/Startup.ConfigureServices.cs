using ESFA.DAS.EmployerProvideFeedback.Configuration;
using ESFA.DAS.EmployerProvideFeedback.Database;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.Orchestrators;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ESFA.DAS.EmployerProvideFeedback
{
    public partial class Startup
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ExternalLinksConfiguration>(Configuration.GetSection("ExternalLinks"));
            services.Configure<GoogleAnalyticsConfiguration>(Configuration.GetSection("GoogleAnalytics"));
            services.AddTransient<IEmployerFeedbackRepository, EmployerFeedbackRepository>();
            services.AddTransient<EnsureFeedbackNotSubmitted>();
            services.AddTransient<EnsureSessionExists>();
            services.AddDatabaseRegistration(Configuration, _hostingEnvironment);
            services.AddTransient<ISessionService, SessionService>();
            services.AddTransient<ReviewAnswersOrchestrator>();
                        
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            var redisString = Configuration.GetConnectionString("RedisApplication");
            if (string.IsNullOrEmpty(redisString))
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddStackExchangeRedisCache(options => {
                    options.Configuration = redisString;
                    options.InstanceName = "";
                });
            }

            services.AddSession(options => {
                options.Cookie.Name = "PF.Session";
                // options.Cookie.HttpOnly = true; // Remove to test pre-prod issue
                options.IdleTimeout = TimeSpan.FromMinutes(60);
            });

            services.AddMvc(options => options.EnableEndpointRouting = false);
        }
    }
}