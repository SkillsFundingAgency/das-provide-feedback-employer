using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ESFA.DAS.EmployerProvideFeedback.Api
{
    public partial class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var configBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", optional: true)
                .AddJsonFile($"appsettings.json.{env.EnvironmentName}", optional: true).AddEnvironmentVariables();

            this.Configuration = configBuilder.Build();
        }

        public IConfiguration Configuration { get; }
    }
}