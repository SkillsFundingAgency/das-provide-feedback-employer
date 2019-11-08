using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Notifications.Api.Client;

namespace ESFA.DAS.Feedback.Employer.Emailer
{
    public class EmployerSurveyReminderEmailer : EmployerSurveyEmailer
    {
        private readonly IStoreEmployerEmailDetails _emailDetailsStore;
        private readonly int _reminderDays;

        public EmployerSurveyReminderEmailer(
            IStoreEmployerEmailDetails emailDetailsStore,
            INotificationsApi emailService,
            IOptions<EmailSettings> settings,
            ILogger<EmployerSurveyEmailer> logger) : base(emailService, logger, settings)
        {
            _emailDetailsStore = emailDetailsStore;
            _reminderDays = settings.Value.ReminderDays;
        }

        public async Task SendEmailsAsync()
        {
            var emailsToSend = await _emailDetailsStore.GetEmployerInvitesToBeSentReminder(_reminderDays);

            // Group by user
            var emailsGroupByUser = GroupEmailsToSendByUser(emailsToSend);

            await SendGroupedEmails(emailsGroupByUser);
        }

        protected override async Task HandleSendAsync(IGrouping<Guid, EmployerSurveyInvite> userGroup)
        {
            var uniqueSurveyCodes = userGroup.Select(x => x.UniqueSurveyCode);
            await SendFeedbackEmail(userGroup, EmailTemplates.ReminderTemplateId);
            await _emailDetailsStore.InsertSurveyInviteHistory(uniqueSurveyCodes, 2);
        }
    }
}
