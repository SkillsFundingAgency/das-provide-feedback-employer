using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ESFA.DAS.EmployerProvideFeedback.Configuration;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.Orchestrators;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.FeedbackDataAccess;
using ESFA.DAS.FeedbackDataAccess.IoC;
using ESFA.DAS.ProvideFeedback.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddTransient<IStoreEmployerEmailDetails, EmployerEmailDetailRepository>();
            services.AddTransient<EnsureFeedbackNotSubmitted>();
            services.AddTransient<EnsureSessionExists>();
            services.Configure<List<ProviderAttributeModel>>(Configuration.GetSection("ProviderAttributes"));
            services.Configure<CosmosConnectionSettings>(Configuration.GetSection("CosmosConnectionSettings"));
            services.AddTransient<IDbConnection>(c => new SqlConnection(Configuration.GetConnectionString("EmployerEmailStoreConnection")));
            services.AddTransient<ISessionService, SessionService>();
            services.AddTransient<ReviewAnswersOrchestrator>();
            services.AddProvideFeedbackCosmos(Configuration);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDistributedRedisCache(options => {
                options.Configuration = Configuration.GetConnectionString("RedisApplication");
                options.InstanceName = "";
            });

            services.AddSession(options => {
                options.Cookie.Name = "PF.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = _hostingEnvironment.IsDevelopment() ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
                options.IdleTimeout = TimeSpan.FromMinutes(60);
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }
    }
}