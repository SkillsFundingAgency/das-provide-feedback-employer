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
    public class InviteEmailerTests
    {
        public class UserWithSingleEntry
        {
            private readonly Mock<IEmployerFeedbackRepository> _mockStore = new Mock<IEmployerFeedbackRepository>();
            private readonly Mock<IMessageSession> _mockEmailService = new Mock<IMessageSession>();
            private readonly Mock<ILogger<EmployerSurveyInviteEmailer>> _mockLogger = new Mock<ILogger<EmployerSurveyInviteEmailer>>();
            private readonly IOptions<EmailSettings> _options;
            private readonly EmployerSurveyInviteEmailer _emailer;

            public UserWithSingleEntry()
            {
                _options = Options.Create(new EmailSettings { FeedbackSiteBaseUrl = "https://test-site.com/", BatchSize = 10 });

                var emailDetails = new List<EmployerSurveyInvite>
                {
                    new EmployerSurveyInvite { UserRef = new Guid("7f11a6b0-a25b-45a5-bdfc-4424dfba85e8"), EmailAddress = "test@test.com" }
                };

                _mockStore.Setup(x => x.GetEmployerUsersToBeSentInvite()).ReturnsAsync(emailDetails);

                _emailer = new EmployerSurveyInviteEmailer(_mockStore.Object, _mockEmailService.Object, _options, _mockLogger.Object);
            }

            [Fact]
            public async Task EmailUsingSingleLinkTemplate()
            {
                await _emailer.SendEmailsAsync();

                _mockEmailService.Verify(x => x.Send(It.Is<SendEmailCommand>(a => a.TemplateId == EmailTemplates.MultipleLinkTemplateId), It.IsAny<SendOptions>()));
            }

            [Fact]
            public async Task EmailedUsingEmailAddressFromStore()
            {
                await _emailer.SendEmailsAsync();

                _mockEmailService.Verify(x => x.Send(It.Is<SendEmailCommand>(a => a.RecipientsAddress == "test@test.com"), It.IsAny<SendOptions>()), Times.Once);
            }

            [Fact]
            public async Task StoreReflectsThatEmailSent()
            {
                await _emailer.SendEmailsAsync();

                _mockStore.Verify(x => x.InsertSurveyInviteHistory(It.IsAny<IEnumerable<Guid>>(), It.IsAny<int>()), Times.Once);
            }
        }

        public class UserWithMultipleEntries
        {
            private Mock<IEmployerFeedbackRepository> _mockStore = new Mock<IEmployerFeedbackRepository>();
            private Mock<IMessageSession> _mockEmailService = new Mock<IMessageSession>();
            private readonly Mock<ILogger<EmployerSurveyInviteEmailer>> _mockLogger = new Mock<ILogger<EmployerSurveyInviteEmailer>>();

            private readonly IOptions<EmailSettings> _options;
            private readonly EmployerSurveyInviteEmailer _emailer;

            public UserWithMultipleEntries()
            {
                _options = Options.Create(new EmailSettings { FeedbackSiteBaseUrl = "https://test-site.com/", BatchSize = 10 });

                var emailDetails = new List<EmployerSurveyInvite>
                {
                    new EmployerSurveyInvite { UserRef = new Guid("7f11a6b0-a25b-45a5-bdfc-4424dfba85e8"), EmailAddress = "test@test.com" },
                    new EmployerSurveyInvite { UserRef = new Guid("7f11a6b0-a25b-45a5-bdfc-4424dfba85e8"), EmailAddress = "test@test.com" }
                };

                _mockStore.Setup(x => x.GetEmployerUsersToBeSentInvite()).ReturnsAsync(emailDetails);

                _emailer = new EmployerSurveyInviteEmailer(_mockStore.Object, _mockEmailService.Object, _options, _mockLogger.Object);

            }

            [Fact]
            public async Task EmailUsingMultipleLinkTemplate()
            {
                await _emailer.SendEmailsAsync();

                _mockEmailService.Verify(x => x.Send(It.Is<SendEmailCommand>(a => a.TemplateId == EmailTemplates.MultipleLinkTemplateId), It.IsAny<SendOptions>()), Times.Once);
            }

            [Fact]
            public async Task EmailedUsingEmailAddressFromStore()
            {
                await _emailer.SendEmailsAsync();

                _mockEmailService.Verify(x => x.Send(It.Is<SendEmailCommand>(a => a.RecipientsAddress == "test@test.com"), It.IsAny<SendOptions>()));
            }

            [Fact]
            public async Task StoreReflectsThatEmailSent()
            {
                await _emailer.SendEmailsAsync();

                _mockStore.Verify(x => x.InsertSurveyInviteHistory(It.IsAny < IEnumerable<Guid>>(), It.IsAny<int>()), Times.Once);
            }
        }
    }
}
