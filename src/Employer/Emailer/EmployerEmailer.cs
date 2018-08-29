using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Esfa.Das.Feedback.Employer.Emailer.Configuration;
using Esfa.Das.ProvideFeedback.Domain.Entities;
using ESFA.DAS.ProvideFeedback.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Types;

namespace Esfa.Das.Feedback.Employer.Emailer
{
    public class EmployerEmailer
    {
        private readonly IStoreEmployerEmailDetails _emailDetailsStore;
        private readonly INotificationsApi _emailService;
        private readonly ILogger<EmployerEmailer> _logger;
        private readonly string _feedbackBaseUrl;

        public EmployerEmailer(IStoreEmployerEmailDetails emailDetailsStore, INotificationsApi emailService, IOptions<EmailSettings> settings, ILogger<EmployerEmailer> logger)
        {
            _emailDetailsStore = emailDetailsStore;
            _emailService = emailService;
            _logger = logger;
            _feedbackBaseUrl = settings.Value.FeedbackSiteBaseUrl.Last() != '/' ? settings.Value.FeedbackSiteBaseUrl + "/" : settings.Value.FeedbackSiteBaseUrl;
        }

        public async Task SendEmailsAsync()
        {
            var emailsToSend = await _emailDetailsStore.GetEmailDetailsToBeSent(100);

            // Group by user
            var users =
            from detail in emailsToSend
            group detail by detail.UserRef into userGroup
            select userGroup;

            foreach (var userGroup in users)
            {
                if (userGroup.Count() > 1)
                {
                    SendMultiLinkEmail(userGroup);
                    await _emailDetailsStore.SetEmailDetailsAsSent(userGroup.Select(x => x.EmailCode));
                }
                else
                {
                    var userDetails = userGroup.Single();
                    await SendSingleLinkEmailAsync(userDetails);
                    await _emailDetailsStore.SetEmailDetailsAsSent(userDetails.EmailCode);
                }
            }
        }

        private async Task SendSingleLinkEmailAsync(EmployerEmailDetail employerEmailDetail)
        {
            var email = new Email
            {
                SystemId = "employer-feedback",
                TemplateId = EmailTemplates.SingleLinkTemplateId,
                Subject = "not-set",
                RecipientsAddress = employerEmailDetail.EmailAddress,
                ReplyToAddress = "not-set",
                Tokens = new Dictionary<string, string>
                    {
                        {"provider_name", employerEmailDetail.ProviderName},
                        {"first_name", employerEmailDetail.UserFirstName},
                        {"feedback_url", $"{_feedbackBaseUrl}{employerEmailDetail.EmailCode}"}
                    }
            };

            try
            {
                await _emailService.SendEmail(email);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Unable to send email for user: {employerEmailDetail.EmailAddress}");
                throw;
            }
        }

        private void SendMultiLinkEmail(IGrouping<Guid, EmployerEmailDetail> userGroup)
        {
            var email = new Email
            {
                SystemId = "",
                TemplateId = EmailTemplates.MultipleLinkTemplateId,
                Subject = "not-set",
                RecipientsAddress = userGroup.First().EmailAddress,
                ReplyToAddress = "not-set",
                Tokens = new Dictionary<string, string>
                    {
                        {"provider_name", ""},
                        {"first_name", ""},
                        {"feedback_url", ""}
                    }
            };

            _emailService.SendEmail(email).Wait();
        }
    }
}