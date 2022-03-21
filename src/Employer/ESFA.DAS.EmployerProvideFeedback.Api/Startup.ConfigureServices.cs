using ESFA.DAS.EmployerProvideFeedback.Api.Configuration;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ESFA.DAS.EmployerProvideFeedback.Api
{

    /// <summary>
    /// The ConfigureServices method
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    public partial class Startup
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        public IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddAuthentication(_configuration, _hostingEnvironment);
            services.AddDatabaseConnection(_configuration, _hostingEnvironment);
            services.AddMediatR(typeof(Startup));
            services.AddTransient<IEmployerFeedbackRepository, EmployerFeedbackRepository>();
            services.AddSwaggerGen();
            services.AddControllers();
            services.AddHealthChecks();
        }
    }
}