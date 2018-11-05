using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Domain.Entities;
using ESFA.DAS.ProvideFeedback.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Types;
using Xunit;

namespace Esfa.Das.Feedback.Employer.UnitTests
{
    public class EmployerEmailTests
    {
        public class UserWithSingleEntry
        {
            private readonly Mock<IStoreEmployerEmailDetails> _mockStore = new Mock<IStoreEmployerEmailDetails>();
            private readonly Mock<INotificationsApi> _mockEmailService = new Mock<INotificationsApi>();
            private readonly Mock<ILogger<EmployerEmailer>> _mockLogger = new Mock<ILogger<EmployerEmailer>>();
            private readonly IOptions<EmailSettings> _options;
            private readonly EmployerEmailer _emailer;

            public UserWithSingleEntry()
            {
                _options = Options.Create(new EmailSettings { FeedbackSiteBaseUrl = "https://test-site.com/", BatchSize = 10 });

                var emailDetails = new List<EmployerEmailDetail>
                {
                    new EmployerEmailDetail { UserRef = new Guid("7f11a6b0-a25b-45a5-bdfc-4424dfba85e8"), EmailAddress = "test@test.com" }
                };

                _mockStore.Setup(x => x.GetEmailDetailsToBeSent()).ReturnsAsync(emailDetails);

                _emailer = new EmployerEmailer(_mockStore.Object, _mockEmailService.Object, _options, _mockLogger.Object);
            }

            [Fact]
            public async Task EmailUsingSingleLinkTemplate()
            {
                await _emailer.SendEmailsAsync();

                _mockEmailService.Verify(x => x.SendEmail(It.Is<Email>(a => a.TemplateId == EmailTemplates.SingleLinkTemplateId)));
            }

            [Fact]
            public async Task EmailedUsingEmailAddressFromStore()
            {
                await _emailer.SendEmailsAsync();

                _mockEmailService.Verify(x => x.SendEmail(It.Is<Email>(a => a.RecipientsAddress == "test@test.com")), Times.Once);
            }

            [Fact]
            public async Task StoreReflectsThatEmailSent()
            {
                await _emailer.SendEmailsAsync();

                _mockStore.Verify(x => x.SetEmailDetailsAsSent(It.IsAny<Guid>()), Times.Once);
            }
        }

        public class UserWithMultipleEntries
        {
            private Mock<IStoreEmployerEmailDetails> _mockStore = new Mock<IStoreEmployerEmailDetails>();
            private Mock<INotificationsApi> _mockEmailService = new Mock<INotificationsApi>();
            private readonly Mock<ILogger<EmployerEmailer>> _mockLogger = new Mock<ILogger<EmployerEmailer>>();

            private readonly IOptions<EmailSettings> _options;
            private readonly EmployerEmailer _emailer;

            public UserWithMultipleEntries()
            {
                _options = Options.Create(new EmailSettings { FeedbackSiteBaseUrl = "https://test-site.com/", BatchSize = 10 });

                var emailDetails = new List<EmployerEmailDetail>
                {
                    new EmployerEmailDetail { UserRef = new Guid("7f11a6b0-a25b-45a5-bdfc-4424dfba85e8"), EmailAddress = "test@test.com" },
                    new EmployerEmailDetail { UserRef = new Guid("7f11a6b0-a25b-45a5-bdfc-4424dfba85e8"), EmailAddress = "test@test.com" }
                };

                _mockStore.Setup(x => x.GetEmailDetailsToBeSent()).ReturnsAsync(emailDetails);

                _emailer = new EmployerEmailer(_mockStore.Object, _mockEmailService.Object, _options, _mockLogger.Object);

            }

            [Fact]
            public async Task EmailUsingMultipleLinkTemplate()
            {
                await _emailer.SendEmailsAsync();

                _mockEmailService.Verify(x => x.SendEmail(It.Is<Email>(a => a.TemplateId == EmailTemplates.MultipleLinkTemplateId)), Times.Once);
            }

            [Fact]
            public async Task EmailedUsingEmailAddressFromStore()
            {
                await _emailer.SendEmailsAsync();

                _mockEmailService.Verify(x => x.SendEmail(It.Is<Email>(a => a.RecipientsAddress == "test@test.com")));
            }

            [Fact]
            public async Task StoreReflectsThatEmailSent()
            {
                await _emailer.SendEmailsAsync();

                _mockStore.Verify(x => x.SetEmailDetailsAsSent(It.IsAny<IEnumerable<Guid>>()), Times.Once);
            }
        }
    }
}
