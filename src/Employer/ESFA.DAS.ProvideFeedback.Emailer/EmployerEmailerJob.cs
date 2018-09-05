using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

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

        public async Task EmailEmployerFeedbackInvitations()
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