namespace ESFA.DAS.EmployerProvideFeedback.Api
{
    using System.Reflection;
    using System.Text;

    using ESFA.DAS.EmployerProvideFeedback.Api.Models;
    using ESFA.DAS.EmployerProvideFeedback.Api.Repository;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;

    using NJsonSchema;

    using NSwag.AspNetCore;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();

            // Enable the Swagger UI middleware and the Swagger generator
            app.UseSwaggerUi(
                typeof(Startup).GetTypeInfo().Assembly,
                settings =>
                    {
                        settings.GeneratorSettings.DefaultPropertyNameHandling = PropertyNameHandling.CamelCase;
                    });

            app.UseAuthentication();

            app.UseMvc();
        }

        public void ConfigureServices(IServiceCollection services)
        {
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
            services.AddDbContext<EmployerFeedbackTestContext>(opt => opt.UseInMemoryDatabase("EmployerFeedback"));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton<IDataRepository>((svc) => 
                    {
                        string endpoint = this.Configuration["Azure:CosmosEndpoint"];
                        string authKey = this.Configuration["Azure:CosmosKey"];
                        string database = this.Configuration["Azure:DatabaseName"];
                        string collection = this.Configuration["Azure:EmployerFeedbackCollection"];

                        return CosmosDbRepository
                            .Instance
                            .ConnectTo(endpoint)
                            .WithAuthKeyOrResourceToken(authKey)
                            .UsingDatabase(database)
                            .UsingCollection(collection);
                    });
        }
    }
}