using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NServiceBus;
using SFA.DAS.Notifications.Messages.Commands;

namespace ESFA.DAS.Feedback.Employer.Emailer
{
    public abstract class EmployerSurveyEmailer
    {
        private readonly IMessageSession _messageSession;
        private readonly ILogger<EmployerSurveyEmailer> _logger;
        private readonly string _feedbackBaseUrl;
        private readonly int _numberOfEmailsToSend;

        public EmployerSurveyEmailer(IMessageSession messageSession, ILogger<EmployerSurveyEmailer> logger, IOptions<EmailSettings> settings)
        {
            _messageSession = messageSession;
            _logger = logger;
            _feedbackBaseUrl = settings.Value.FeedbackSiteBaseUrl.Last() != '/' ? settings.Value.FeedbackSiteBaseUrl + "/" : settings.Value.FeedbackSiteBaseUrl;
            _numberOfEmailsToSend = settings.Value.BatchSize;
        }

        protected async Task SendFeedbackEmail(IGrouping<Guid, EmployerSurveyInvite> userGroup, string templateId)
        {
            var emailAddress = userGroup.First().EmailAddress;
            var feedbackUrlStrings = userGroup.Select(employerEmailDetail => $"{employerEmailDetail.ProviderName} {Environment.NewLine} {_feedbackBaseUrl}{employerEmailDetail.UniqueSurveyCode}");
            var feedbackUrls = string.Join("\r\n \r\n", feedbackUrlStrings);

            var tokens = new Dictionary<string, string>
                    {
                        {"provider_name", userGroup.First().ProviderName},
                        { "first_name", userGroup.First().FirstName},
                        {"feedback_urls", feedbackUrls}
                    };

            var email = new SendEmailCommand(templateId, userGroup.First().EmailAddress, tokens);
            await SendEmail(emailAddress, email);
        }

        protected IEnumerable<IGrouping<Guid, EmployerSurveyInvite>> GroupEmailsToSendByUser(IEnumerable<EmployerSurveyInvite> emailsToSend)
        {
            return emailsToSend
                .GroupBy(email => email.UserRef)
                .Take(_numberOfEmailsToSend);
        }

        protected async Task SendGroupedEmails(IEnumerable<IGrouping<Guid, EmployerSurveyInvite>> emailsGroupByUser)
        {
            foreach (var userGroup in emailsGroupByUser)
            {
                await HandleSendAsync(userGroup);
            }
        }

        protected abstract Task HandleSendAsync(IGrouping<Guid, EmployerSurveyInvite> userGroup);

        private async Task SendEmail(string sendToAddress, SendEmailCommand email)
        {
            try
            {
                _logger.LogInformation($"Sending email to {sendToAddress}");
                await _messageSession.Send(email);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Unable to send email for user: {sendToAddress}");
                throw;
            }
        }
    }
}
