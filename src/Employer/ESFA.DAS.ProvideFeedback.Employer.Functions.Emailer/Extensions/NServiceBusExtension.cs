using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using System;
using System.IO;
using System.Threading.Tasks;
using EndpointConfiguration = NServiceBus.EndpointConfiguration;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.Extensions
{
    public static class AddNServiceBusExtension
    {
        public static async Task AddNServiceBus(this IFunctionsHostBuilder builder, IConfigurationRoot configuration)
        {
            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerFeedback.Emailer")
                .UseMessageConventions()
                .UseNewtonsoftJsonSerializer()
                .UseSendOnly();

            var nServiceBusConfig = configuration.GetSection("NServiceBusConfiguration").Get<NServiceBusConfiguration>();

            if (nServiceBusConfig.ConnectionString.Equals("UseLearningEndpoint=true", StringComparison.CurrentCultureIgnoreCase))
            {
                var transport = endpointConfiguration.UseTransport<LearningTransport>();
                transport.StorageDirectory(Path.Combine(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src")), ".learningtransport"));
                endpointConfiguration.UseLearningTransport(s => s.AddRouting());
            }
            else
            {
                endpointConfiguration.UseAzureServiceBusTransport(nServiceBusConfig.ConnectionString, r => r.AddRouting());
            }

            if (!string.IsNullOrEmpty(nServiceBusConfig.License))
            {
                endpointConfiguration.License(nServiceBusConfig.License);
            }

            var endpoint = await Endpoint.Start(endpointConfiguration);

            builder.Services.AddSingleton(p => endpoint)
                .AddSingleton<IMessageSession>(p => p.GetService<IEndpointInstance>());
        }
    }
}