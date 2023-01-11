﻿namespace ESFA.DAS.EmployerProvideFeedback.Api
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using NLog.Web;
    using System;

    public class Program
    {
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).UseApplicationInsights().UseStartup<Startup>().UseNLog()
                .UseUrls("https://localhost:5031");

        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Info("Starting up host");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                // NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
        }
    }
}