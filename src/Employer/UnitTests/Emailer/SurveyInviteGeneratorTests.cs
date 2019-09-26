using System;
using System.Threading.Tasks;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using ESFA.DAS.ProvideFeedback.Employer.Application;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace UnitTests.Emailer
{
    public class SurveyInviteGeneratorTests
    {
        private readonly SurveyInviteGenerator _sut;
        private readonly Mock<IStoreEmployerEmailDetails> _emailDetailsRepoMock;
        private readonly GenerateSurveyCodeMessage _message;
        private readonly EmailSettings _testConfig;

        public SurveyInviteGeneratorTests()
        {
            _testConfig = new EmailSettings { InviteCycleDays = 90 };
            _message = new GenerateSurveyCodeMessage { AccountId = 1, Ukprn = 88888888, UserRef = Guid.NewGuid() };
            _emailDetailsRepoMock = new Mock<IStoreEmployerEmailDetails>();
            var config = Options.Create(_testConfig);
            _sut = new SurveyInviteGenerator(config, _emailDetailsRepoMock.Object, Mock.Of<ILogger<SurveyInviteGenerator>>());
        }

        [Fact]
        public async Task WhenNoInviteExists_CreateNewSurveyCode()
        {
            // Arrange
            long feedbackId = 88;
            _emailDetailsRepoMock
                .Setup(mock => mock.UpsertIntoFeedback(_message.UserRef, _message.AccountId, _message.Ukprn))
                .ReturnsAsync(feedbackId);

            // Act
            await _sut.GenerateSurveyInvites(_message);

            
            // Assert
            _emailDetailsRepoMock.Verify(mock => mock.InsertNewSurveyForFeedback(feedbackId), Times.Once);
        }

        [Fact]
        public async Task WhenInviteExists_SentLessThanCycle_ShouldNoCreateNewSurveyCode()
        {
            // Arrange
            long feedbackId = 88;
            _emailDetailsRepoMock
                .Setup(mock => mock.UpsertIntoFeedback(_message.UserRef, _message.AccountId, _message.Ukprn))
                .ReturnsAsync(feedbackId);

            var inviteLastSentDate = DateTime.UtcNow.AddDays(-(_testConfig.InviteCycleDays - 1));

            _emailDetailsRepoMock
                .Setup(mock => mock.GetEmployerSurveyInvite(feedbackId))
                .ReturnsAsync(new EmployerSurveyInvite { InviteSentDate = inviteLastSentDate });

            // Act
            await _sut.GenerateSurveyInvites(_message);


            // Assert
            _emailDetailsRepoMock.Verify(mock => mock.InsertNewSurveyForFeedback(feedbackId), Times.Never);
        }

        [Fact]
        public async Task WhenInviteExists_SentMoreThanCycle_ShouldNoCreateNewSurveyCode()
        {
            // Arrange
            long feedbackId = 88;
            _emailDetailsRepoMock
                .Setup(mock => mock.UpsertIntoFeedback(_message.UserRef, _message.AccountId, _message.Ukprn))
                .ReturnsAsync(feedbackId);

            var inviteLastSentDate = DateTime.UtcNow.AddDays(-(_testConfig.InviteCycleDays + 1));

            _emailDetailsRepoMock
                .Setup(mock => mock.GetEmployerSurveyInvite(feedbackId))
                .ReturnsAsync(new EmployerSurveyInvite { InviteSentDate = inviteLastSentDate });

            // Act
            await _sut.GenerateSurveyInvites(_message);


            // Assert
            _emailDetailsRepoMock.Verify(mock => mock.InsertNewSurveyForFeedback(feedbackId), Times.Once);
        }
    }
}
