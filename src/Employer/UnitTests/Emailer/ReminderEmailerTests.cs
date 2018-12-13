using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Types;
using Xunit;

namespace Esfa.Das.Feedback.Employer.UnitTests.Emailer
{
    public class ReminderEmailerTests
    {
        private readonly Mock<IStoreEmployerEmailDetails> _mockStore = new Mock<IStoreEmployerEmailDetails>();
        private readonly Mock<INotificationsApi> _mockEmailService = new Mock<INotificationsApi>();
        private readonly Mock<ILogger<EmployerSurveyEmailer>> _mockLogger = new Mock<ILogger<EmployerSurveyEmailer>>();
        private readonly IOptions<EmailSettings> _options;
        private readonly EmployerSurveyReminderEmailer _emailer;

        public ReminderEmailerTests()
        {
            _options = Options.Create(new EmailSettings { FeedbackSiteBaseUrl = "https://test-site.com/", BatchSize = 10 });

            var emailDetails = new List<EmployerEmailDetail>
                {
                    new EmployerEmailDetail { UserRef = new Guid("7f11a6b0-a25b-45a5-bdfc-4424dfba85e8"), EmailAddress = "test@test.com" }
                };

            _mockStore.Setup(x => x.GetEmailDetailsToBeSentReminder()).ReturnsAsync(emailDetails);

            _emailer = new EmployerSurveyReminderEmailer(_mockStore.Object, _mockEmailService.Object, _options, _mockLogger.Object);
        }

        [Fact]
        public async Task EmailUsingReminderTemplate()
        {
            await _emailer.SendEmailsAsync();

            _mockEmailService.Verify(x => x.SendEmail(It.Is<Email>(a => a.TemplateId == EmailTemplates.ReminderTemplateId)));
        }

        [Fact]
        public async Task EmailedUsingEmailAddressFromStore()
        {
            await _emailer.SendEmailsAsync();

            _mockEmailService.Verify(x => x.SendEmail(It.Is<Email>(a => a.RecipientsAddress == "test@test.com")), Times.Once);
        }

        [Fact]
        public async Task StoreReflectsThatEmailReminderSent()
        {
            await _emailer.SendEmailsAsync();

            _mockStore.Verify(x => x.SetEmailReminderAsSent(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task NoEmailsToSendNoEmailSent()
        {
            _mockStore.Reset();
            await _emailer.SendEmailsAsync();

            _mockEmailService.Verify(x => x.SendEmail(It.IsAny<Email>()), Times.Never);
        }

        [Fact]
        public async Task NoEmailsToSendNoStoreUpdate()
        {
            _mockStore.Reset();
            await _emailer.SendEmailsAsync();

            _mockStore.Verify(x => x.SetEmailDetailsAsSent(It.IsAny<Guid>()), Times.Never);
        }
    }
}
