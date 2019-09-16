namespace ESFA.DAS.EmployerProvideFeedback.Api
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using AutoMapper;

    using ESFA.DAS.EmployerProvideFeedback.Api.Configuration;
    using ESFA.DAS.EmployerProvideFeedback.Api.Repository;

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// The ConfigureServices method
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    public partial class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton<IEmployerFeedbackRepository>(
                (svc) =>
                    {
                        string endpoint = this.Configuration["Azure:CosmosEndpoint"];
                        string authKey = this.Configuration["Azure:CosmosKey"];
                        string database = this.Configuration["Azure:DatabaseName"];
                        string collection = this.Configuration["Azure:EmployerFeedbackCollection"];

                        return CosmosEmployerFeedbackRepository.Instance.ConnectTo(endpoint).WithAuthKeyOrResourceToken(authKey)
                            .UsingDatabase(database).UsingCollection(collection);
                    });

            services.Configure<AzureOptions>(this.Configuration.GetSection("Azure"));
            services.Configure<AzureAdOptions>(Configuration.GetSection("AzureAd"));

            var serviceProvider = services.BuildServiceProvider();

            var azureActiveDirectoryConfiguration =
                    serviceProvider.GetService<IOptions<AzureAdOptions>>();
            services.AddAuthentication(auth => { auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; })
                    .AddJwtBearer(auth =>
                    {
                        auth.Authority =
                            $"https://login.microsoftonline.com/{azureActiveDirectoryConfiguration.Value.Tenant}";
                        auth.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidAudiences = new List<string>
                            {
                                azureActiveDirectoryConfiguration.Value.Identifier
                            }
                        };
                    });
        }
    }
}