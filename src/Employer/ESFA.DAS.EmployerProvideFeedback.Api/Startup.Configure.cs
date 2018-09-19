namespace ESFA.DAS.EmployerProvideFeedback.Api
{
    using System.Reflection;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;

    using NJsonSchema;

    using NSwag;
    using NSwag.AspNetCore;
    using NSwag.SwaggerGeneration.Processors.Security;

    /// <summary>
    /// The configure method.
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    public partial class Startup
    {
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            IApplicationLifetime applicationLifetime,
            ILogger<Startup> logger)
        {
            applicationLifetime.ApplicationStarted.Register(() => logger.LogInformation("Host fully started"));
            applicationLifetime.ApplicationStopping.Register(
                () => logger.LogInformation("Host shutting down...waiting to complete requests."));
            applicationLifetime.ApplicationStopped.Register(
                () => logger.LogInformation("Host fully stopped. All requests processed."));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //else
            //{
            //    app.UseExceptionHandler("/error/handle");
            //    app.UseHsts();
            //}

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Enable the Swagger UI middleware and the Swagger generator
            app.UseSwaggerUi(
                typeof(Startup).GetTypeInfo().Assembly,
                settings =>
                    {
                        settings.GeneratorSettings.DefaultPropertyNameHandling = PropertyNameHandling.CamelCase;

                        settings.GeneratorSettings.OperationProcessors.Add(
                            new OperationSecurityScopeProcessor("JWT Token"));

                        settings.GeneratorSettings.DocumentProcessors.Add(
                            new SecurityDefinitionAppender(
                                "JWT Token",
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
    }
}