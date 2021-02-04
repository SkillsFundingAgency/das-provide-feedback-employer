using Microsoft.Extensions.Hosting;

namespace ESFA.DAS.EmployerProvideFeedback.Api
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The configure method.
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    public partial class Startup
    {
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IHostApplicationLifetime applicationLifetime,
            ILogger<Startup> logger)
        {
            applicationLifetime.ApplicationStarted.Register(() => logger.LogInformation("Host fully started"));
            applicationLifetime.ApplicationStopping.Register(() => logger.LogInformation("Host shutting down...waiting to complete requests."));
            applicationLifetime.ApplicationStopped.Register(() => logger.LogInformation("Host fully stopped. All requests processed."));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Enable the Swagger UI middleware and the Swagger generator
            app.UseOpenApi();
            app.UseSwaggerUi3();
            app.UseAuthentication();
            app.UseHealthChecks("/health");
        }
    }
}