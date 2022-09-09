using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NServiceBus;

namespace ESFA.DAS.Feedback.Employer.Emailer
{
    public class EmployerSurveyReminderEmailer : EmployerSurveyEmailer
    {
        private readonly IEmployerFeedbackRepository _emailDetailsStore;
        private readonly int _reminderDays;

        public EmployerSurveyReminderEmailer(
            IEmployerFeedbackRepository emailDetailsStore,
            IMessageSession _messageSession,
            IOptions<EmailSettings> settings,
            ILogger<EmployerSurveyEmailer> logger) : base(_messageSession, logger, settings)
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
