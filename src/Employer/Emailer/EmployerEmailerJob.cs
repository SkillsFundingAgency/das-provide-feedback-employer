using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Esfa.Das.Feedback.Employer.Emailer.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Client.Configuration;
using SFA.DAS.Notifications.Api.Types;

namespace Esfa.Das.Feedback.Employer.Emailer
{
    public class EmployerEmailerJob
    {
        private readonly ILogger<EmployerEmailerJob> _logger;
        private readonly EmployerEmailer _emailer;

        public EmployerEmailerJob(ILogger<EmployerEmailerJob> logger, EmployerEmailer emailer)
        {
            _logger = logger;
            _emailer = emailer;
        }

        public async Task EmailEmployerFeedbackInvitations([TimerTrigger(Schedules.FourAmDaily, RunOnStartup = true)] TimerInfo timerInfo, TextWriter log)
        {
            _logger.LogInformation("Starting employer emailer job.");

            try
            {
                await _emailer.SendEmailsAsync();

                _logger.LogInformation("Finished emailing employers.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to email employers.");
                throw;
            }
        }
    }
}