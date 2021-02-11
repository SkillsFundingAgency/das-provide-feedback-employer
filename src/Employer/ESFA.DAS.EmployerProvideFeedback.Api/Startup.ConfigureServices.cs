namespace ESFA.DAS.EmployerProvideFeedback.Api
{
    using AutoMapper;

    using Configuration;
    using Repository;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
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

            services.AddControllers();

            services.AddSingleton<IEmployerFeedbackRepository>(
                (svc) =>
                {
                    string endpoint = this.Configuration["Azure:CosmosEndpoint"];
                    string authKey = this.Configuration["Azure:CosmosKey"];
                    string database = this.Configuration["Azure:DatabaseName"];
                    string collection = this.Configuration["Azure:EmployerFeedbackCollection"];

                    return CosmosEmployerFeedbackRepository.Instance.ConnectTo(endpoint)
                        .WithAuthKeyOrResourceToken(authKey)
                        .UsingDatabase(database).UsingCollection(collection);
                });

            services.Configure<AzureOptions>(this.Configuration.GetSection("Azure"));
            services.Configure<AzureAdOptions>(Configuration.GetSection("AzureAd"));
            services.AddMvc();
            services.AddSwaggerDocument();
            services.AddHealthChecks();

            var serviceProvider = services.BuildServiceProvider();
            var azureActiveDirectoryConfiguration = serviceProvider.GetService<IOptions<AzureAdOptions>>();

            services.AddAuthentication(auth => { auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; })
                .AddJwtBearer(auth =>
                {
                    auth.Authority = $"https://login.microsoftonline.com/{azureActiveDirectoryConfiguration.Value.Tenant}";
                    auth.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAudiences = azureActiveDirectoryConfiguration.Value.Identifier.Split(',')
                    };
                });
        }
    }
}