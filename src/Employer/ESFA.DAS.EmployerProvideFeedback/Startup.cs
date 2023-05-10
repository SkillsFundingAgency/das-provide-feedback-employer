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
using SFA.DAS.Employer.Shared.UI;
using Microsoft.AspNetCore.Mvc;
using ESFA.DAS.EmployerProvideFeedback.Attributes.ModelBinders;

namespace ESFA.DAS.EmployerProvideFeedback
{
    public class Startup
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        public IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _configuration = configuration;
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

            _configuration = config.Build();
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
            services.AddConfigurationOptions(_configuration);
            var config = _configuration.GetSection("ProvideFeedbackEmployerWeb").Get<ProvideFeedbackEmployerWebConfiguration>();

            services.AddApplicationInsightsTelemetry(_configuration.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY"));
            services.AddDatabaseRegistration(config, _hostingEnvironment);
            services.AddEmployerAuthentication(config, _configuration);
            services.AddEmployerSharedUI(config, _configuration);
            services.AddMemoryCache();
            services.AddCache(_hostingEnvironment, config);
            services.AddDasDataProtection(config, _hostingEnvironment);
            services.AddServiceRegistrations(config);
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