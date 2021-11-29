using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ESFA.DAS.EmployerProvideFeedback.Database
{
    public static class AddDatabaseExtension
    {
        public static void AddDatabaseRegistration(this IServiceCollection services, IConfiguration configuration, IHostingEnvironment environment)
        {
            services.AddTransient<IDbConnection>(c =>
            {
                const string azureResource = "https://database.windows.net/";
                string connectionString = configuration.GetConnectionString("EmployerEmailStoreConnection");

                if (environment.IsDevelopment())
                    return new SqlConnection(connectionString);

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
