using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Configuration.AzureTableStorage;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.StartUpExtensions
{
    public static class ConfigurationExtensions
    {
        public static IConfiguration BuildDasConfiguration(this IConfigurationBuilder configBuilder, IConfiguration configuration)
        {
            configBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

#if DEBUG
            configBuilder.AddJsonFile("local.settings.json", optional: true);
#endif
            //configBuilder.AddAzureTableStorage(options =>
            //{
            //    options.ConfigurationKeys = ["SFA.DAS.EarlyConnect.Jobs"];
            //    options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
            //    options.EnvironmentName = configuration["EnvironmentName"];
            //    options.PreFixConfigurationKeys = false;
            //});

            return configBuilder.Build();
        }
    }
}