using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.Services
{
    internal class SettingsProvider : ISettingService
    {
        private readonly IConfigurationRoot config;

        public SettingsProvider(ExecutionContext ctx)
        {
            this.config = new ConfigurationBuilder()
                .SetBasePath(ctx.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public string Get(string parameterName)
        {
            var parameter = this.config.GetConnectionStringOrSetting(parameterName);
            return parameter;
        }
    }
}
