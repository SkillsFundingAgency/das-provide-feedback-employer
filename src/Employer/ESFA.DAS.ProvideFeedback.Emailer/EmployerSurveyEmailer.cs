using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Types;

namespace ESFA.DAS.Feedback.Employer.Emailer
{
    public abstract class EmployerSurveyEmailer
    {
        private readonly INotificationsApi _emailService;
        private readonly ILogger<EmployerSurveyEmailer> _logger;
        private readonly string _feedbackBaseUrl;
        private readonly int _numberOfEmailsToSend;

        public EmployerSurveyEmailer(INotificationsApi emailService, ILogger<EmployerSurveyEmailer> logger, EmailSettings settings)
        {
            _emailService = emailService;
            _logger = logger;
            _feedbackBaseUrl = settings.FeedbackSiteBaseUrl.Last() != '/' ? settings.FeedbackSiteBaseUrl + "/" : settings.FeedbackSiteBaseUrl;
            _numberOfEmailsToSend = settings.BatchSize;
        }

        protected async Task SendFeedbackEmail(IGrouping<Guid, EmployerEmailDetail> userGroup, string templateId)
        {
            var emailAddress = userGroup.First().EmailAddress;
            var feedbackUrlStrings = userGroup.Select(employerEmailDetail => $"{employerEmailDetail.ProviderName} {Environment.NewLine} {_feedbackBaseUrl}{employerEmailDetail.EmailCode}");
            var feedbackUrls = string.Join("\r\n \r\n", feedbackUrlStrings);

            var email = new Email
            {
                SystemId = "employer-feedback",
                TemplateId = templateId,
                Subject = "not-set",
                RecipientsAddress = userGroup.First().EmailAddress,
                ReplyToAddress = "not-set",
                Tokens = new Dictionary<string, string>
                    {
                        {"provider_name", userGroup.First().ProviderName},
                        { "first_name", userGroup.First().UserFirstName},
                        {"feedback_urls", feedbackUrls}
                    }
            };

            await SendEmail(emailAddress, email);
        }

        protected IEnumerable<IGrouping<Guid, EmployerEmailDetail>> GroupEmailsToSendByUser(IEnumerable<EmployerEmailDetail> emailsToSend)
        {
            return emailsToSend
                .GroupBy(email => email.UserRef)
                .Take(_numberOfEmailsToSend);
        }

        protected async Task SendGroupedEmails(IEnumerable<IGrouping<Guid, EmployerEmailDetail>> emailsGroupByUser)
        {
            foreach (var userGroup in emailsGroupByUser)
            {
                await HandleSendAsync(userGroup);
            }
        }

        protected abstract Task HandleSendAsync(IGrouping<Guid, EmployerEmailDetail> userGroup);

        private async Task SendEmail(string sendToAddress, Email email)
        {
            try
            {
                _logger.LogInformation($"Sending email to {sendToAddress}");
                await _emailService.SendEmail(email);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Unable to send email for user: {sendToAddress}");
                throw;
            }
        }
    }
}
