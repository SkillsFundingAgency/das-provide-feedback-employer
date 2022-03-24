using System.Data;
using System.Data.SqlClient;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ESFA.DAS.EmployerProvideFeedback.Database
{
    public static class AddDatabaseExtension
    {
        public static void AddDatabaseRegistration(this IServiceCollection services, ProvideFeedbackEmployerWeb configuration, IWebHostEnvironment environment)
        {
            services.AddTransient<IDbConnection>(c =>
            {
                const string azureResource = "https://database.windows.net/";
                string connectionString = configuration.EmployerFeedbackDatabaseConnectionString;

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
