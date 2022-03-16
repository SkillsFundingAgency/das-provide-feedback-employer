using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using ESFA.DAS.EmployerProvideFeedback.Configuration;
using ESFA.DAS.EmployerProvideFeedback.Database;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.Orchestrators;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Hosting;
using System.IO;
using SFA.DAS.Configuration.AzureTableStorage;
using ESFA.DAS.EmployerProvideFeedback.StartupExtensions;
using SFA.DAS.EmployerUrlHelper.DependencyResolution;
using SFA.DAS.Employer.Shared.UI;
using Microsoft.AspNetCore.Mvc;
using ESFA.DAS.EmployerProvideFeedback.Attributes.ModelBinders;

namespace ESFA.DAS.EmployerProvideFeedback
{
    public class Startup
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            _hostingEnvironment = hostingEnvironment;

            var config = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
#if DEBUG
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
#endif
                .AddEnvironmentVariables();

            if (!configuration["Environment"].Equals("DEV", StringComparison.CurrentCultureIgnoreCase))
            {
                config.AddAzureTableStorage(options =>
                {
                    options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                    options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                    options.EnvironmentName = configuration["Environment"];
                    options.PreFixConfigurationKeys = false;
                }
                );
            }

            Configuration = config.Build();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime, ILogger<Startup> logger)
        {
            applicationLifetime.ApplicationStarted.Register(() => logger.LogInformation("Host fully started"));
            applicationLifetime.ApplicationStopping.Register(() => logger.LogInformation("Host shutting down...waiting to complete requests."));
            applicationLifetime.ApplicationStopped.Register(() => logger.LogInformation("Host fully stopped. All requests processed."));

            app.UseStatusCodePagesWithReExecute("/error/{0}");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error/handle");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
                        
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConfigurationOptions(Configuration);
            var config = Configuration.GetSection<ProvideFeedbackEmployerWeb>();
            services.AddSingleton(config);

            services.AddApplicationInsightsTelemetry(Configuration.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY"));
            services.AddDatabaseRegistration(config, _hostingEnvironment);
            services.AddEmployerAuthentication(Configuration.GetSection<Authentication>());
            services.AddEmployerSharedUI(Configuration);
            services.AddEmployerUrlHelper();
            services.AddMemoryCache();
            services.AddCache(_hostingEnvironment, config);
            services.AddDasDataProtection(config, _hostingEnvironment);
            services.AddServiceRegistrations(Configuration);
            services.AddSessionPersistance();

            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                options.ModelBinderProviders.Insert(0, new AutoDecodeModelBinderProvider());
            })
            .SetDefaultNavigationSection(NavigationSection.AccountsHome);
                
                
        }
    }
}