using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.Database
{
    public static class AddDatabaseExtension
    {
        public static void AddDatabaseRegistration(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddTransient<IDbConnection>(c =>
            {
                const string azureResource = "https://database.windows.net/";
                string connectionString = configuration.GetConnectionStringOrSetting("EmployerEmailStoreConnection");
                var env = configuration.GetConnectionStringOrSetting("ASPNETCORE_ENVIRONMENT");

                if (string.IsNullOrEmpty(env) || env.Equals("development", StringComparison.OrdinalIgnoreCase))
                {
                    return new SqlConnection(connectionString);
                }

                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                return new SqlConnection
                {
                    ConnectionString = connectionString,
                    AccessToken = azureServiceTokenProvider.GetAccessTokenAsync(azureResource).Result
                };

            });
        }
    }
}
