using System.IO;
using ESFA.DAS.EmployerProvideFeedback.Configuration;
using Microsoft.AspNetCore.Hosting;
using NSwag;
using NSwag.SwaggerGeneration.Processors.Security;

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
        public Startup(IHostingEnvironment env)
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", optional: true)
                .AddJsonFile($"appsettings.json.{env.EnvironmentName}", optional: true)
                .AddEnvironmentVariables();

            this.Configuration = configBuilder.Build();
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
                        settings.GeneratorSettings.OperationProcessors.Add(new OperationSecurityScopeProcessor("JWT Token"));

                        settings.GeneratorSettings.DocumentProcessors.Add(new SecurityDefinitionAppender("JWT Token",
                            new SwaggerSecurityScheme
                            {
                                Type = SwaggerSecuritySchemeType.ApiKey,
                                Name = "Authorization",
                                Description = "Copy 'Bearer ' + valid JWT token into field",
                                In = SwaggerSecurityApiKeyLocation.Header
                            }));
                    });

            app.UseAuthentication();

            app.UseMvc();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true);
            var config = configBuilder.Build();

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

            services.Configure<AzureOptions>(Configuration.GetSection("Azure"));
            services.Configure<JwtOptions>(Configuration.GetSection("Jwt"));
        }
    }
}