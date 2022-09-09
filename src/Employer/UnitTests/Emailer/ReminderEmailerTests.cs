using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NServiceBus;
using SFA.DAS.Notifications.Messages.Commands;
using Xunit;

namespace ESFA.DAS.Feedback.Employer.UnitTests.Emailer
{
    public class ReminderEmailerTests
    {
        private readonly Mock<IEmployerFeedbackRepository> _mockStore = new Mock<IEmployerFeedbackRepository>();
        private readonly Mock<IMessageSession> _mockMessageSession = new Mock<IMessageSession>();
        private readonly Mock<ILogger<EmployerSurveyEmailer>> _mockLogger = new Mock<ILogger<EmployerSurveyEmailer>>();
        private readonly IOptions<EmailSettings> _options;
        private readonly EmployerSurveyReminderEmailer _emailer;

        public ReminderEmailerTests()
        {
            _options = Options.Create(new EmailSettings { FeedbackSiteBaseUrl = "https://test-site.com/", BatchSize = 10 });

            var emailDetails = new List<EmployerSurveyInvite>
                {
                    new EmployerSurveyInvite { UserRef = new Guid("7f11a6b0-a25b-45a5-bdfc-4424dfba85e8"), EmailAddress = "test@test.com" }
                };

            _mockStore.Setup(x => x.GetEmployerInvitesToBeSentReminder(It.IsAny<int>())).ReturnsAsync(emailDetails);

            _emailer = new EmployerSurveyReminderEmailer(_mockStore.Object, _mockMessageSession.Object, _options, _mockLogger.Object);
        }

        [Fact]
        public async Task EmailUsingReminderTemplate()
        {
            await _emailer.SendEmailsAsync();

            _mockMessageSession.Verify(x => x.Send(It.Is<SendEmailCommand>(a => a.TemplateId == EmailTemplates.ReminderTemplateId), It.IsAny<SendOptions>()));
        }

        [Fact]
        public async Task EmailedUsingEmailAddressFromStore()
        {
            await _emailer.SendEmailsAsync();

            _mockMessageSession.Verify(x => x.Send(It.Is<SendEmailCommand>(a => a.RecipientsAddress == "test@test.com"), It.IsAny<SendOptions>()), Times.Once);
        }

        [Fact]
        public async Task StoreReflectsThatEmailReminderSent()
        {
            await _emailer.SendEmailsAsync();

            _mockStore.Verify(x => x.InsertSurveyInviteHistory(It.IsAny<IEnumerable<Guid>>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task NoEmailsToSendNoEmailSent()
        {
            _mockStore.Reset();
            await _emailer.SendEmailsAsync();

            _mockMessageSession.Verify(x => x.Send(It.IsAny<SendEmailCommand>(), It.IsAny<SendOptions>()), Times.Never);
        }

        [Fact]
        public async Task NoEmailsToSendNoStoreUpdate()
        {
            _mockStore.Reset();
            await _emailer.SendEmailsAsync();

            _mockStore.Verify(x => x.InsertSurveyInviteHistory(It.IsAny<IEnumerable<Guid>>(), It.IsAny<int>()), Times.Never);
        }
    }
}
