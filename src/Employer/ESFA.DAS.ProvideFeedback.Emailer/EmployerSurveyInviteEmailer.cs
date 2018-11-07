using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Notifications.Api.Client;

namespace ESFA.DAS.Feedback.Employer.Emailer
{
    public class EmployerSurveyInviteEmailer : EmployerSurveyEmailer
    {
        private readonly IStoreEmployerEmailDetails _emailDetailsStore;

        public EmployerSurveyInviteEmailer(
            IStoreEmployerEmailDetails emailDetailsStore, 
            INotificationsApi emailService, 
            IOptions<EmailSettings> settings, 
            ILogger<EmployerSurveyInviteEmailer> logger) : base(emailService, logger, settings)
        {
            _emailDetailsStore = emailDetailsStore;
        }

        public async Task SendEmailsAsync()
        {
            var emailsToSend = await _emailDetailsStore.GetEmailDetailsToBeSentInvite();

            // Group by user
            var emailsGroupByUser = GroupEmailsToSendByUser(emailsToSend);

            await SendGroupedEmails(emailsGroupByUser);
        }

        protected override async Task HandleSendAsync(IGrouping<Guid, EmployerEmailDetail> userGroup)
        {
            var userRef = userGroup.First().UserRef;
            await SendFeedbackEmail(userGroup, EmailTemplates.MultipleLinkTemplateId);
            await _emailDetailsStore.SetEmailDetailsAsSent(userRef);
        }
    }
}