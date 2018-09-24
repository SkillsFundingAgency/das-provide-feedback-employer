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
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
                options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = this.Configuration["Jwt:Issuer"],
                            ValidAudience = this.Configuration["Jwt:Issuer"],
                            IssuerSigningKey =
                                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Configuration["Jwt:Key"]))
                        };
                    });

            //services.AddAuthentication(sharedOptions =>
            //        {
            //            sharedOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            //        })
            //    .AddAzureADBearer(options => this.Configuration.Bind("AzureAd", options));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton<ITokenProvider>(
                (svc) =>
                    {
                        string endpoint = this.Configuration["Azure:CosmosEndpoint"];
                        string authKey = this.Configuration["Azure:CosmosKey"];
                        string database = this.Configuration["Azure:DatabaseName"];
                        string collection = this.Configuration["Azure:TokenCollection"];

                        return CosmosTokenStore.Instance.ConnectTo(endpoint).WithAuthKeyOrResourceToken(authKey)
                            .UsingDatabase(database).UsingCollection(collection);
                    });

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
            services.Configure<JwtOptions>(this.Configuration.GetSection("Jwt"));
        }
    }
}