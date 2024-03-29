﻿namespace ESFA.DAS.EmployerProvideFeedback.Api
{
    using System.IO;
    using MediatR;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public partial class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var configBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", optional: true)
                .AddJsonFile($"appsettings.json.{env.EnvironmentName}", optional: true).AddEnvironmentVariables();

            this.Configuration = configBuilder.Build();
        }

        public IConfiguration Configuration { get; }
    }
}