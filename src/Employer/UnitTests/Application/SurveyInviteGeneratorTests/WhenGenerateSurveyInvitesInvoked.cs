using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using ESFA.DAS.ProvideFeedback.Employer.Application;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Application.SurveyInviteGeneratorTests
{
    public class WhenGenerateSurveyInvitesInvoked
    {
        private readonly SurveyInviteGenerator _sut;
        private readonly Mock<IStoreEmployerEmailDetails> _emailDetailsRepoMock;
        private readonly GenerateSurveyCodeMessage _message;
        private readonly EmailSettings _testConfig;
        private const int FeedBackId = 88;
        private const int InviteCycleDays = 90;

        public WhenGenerateSurveyInvitesInvoked()
        {
            _testConfig = new EmailSettings { InviteCycleDays = InviteCycleDays };
            var config = Options.Create(_testConfig);

            _message = new GenerateSurveyCodeMessage { AccountId = 1, Ukprn = 88888888, UserRef = Guid.NewGuid() };

            _emailDetailsRepoMock = new Mock<IStoreEmployerEmailDetails>();
            _emailDetailsRepoMock
                .Setup(mock => mock.UpsertIntoFeedback(_message.UserRef, _message.AccountId, _message.Ukprn))
                .ReturnsAsync(FeedBackId);

            _sut = new SurveyInviteGenerator(config, _emailDetailsRepoMock.Object, Mock.Of<ILogger<SurveyInviteGenerator>>());
        }

        [Fact]
        public async Task ThenUpsertFeedback()
        {
            // Arrange            
            _emailDetailsRepoMock
                .Setup(m => m.GetFeedbackInviteSentDateAsync(FeedBackId))
                .ReturnsAsync(new FeedbackInvite());

            // Act
            await _sut.GenerateSurveyInvites(_message);

            // Assert
            _emailDetailsRepoMock.Verify(m => m.UpsertIntoFeedback(_message.UserRef, _message.AccountId, _message.Ukprn), Times.Once);
        }

        [Theory, MemberData(nameof(CreateTestData))]
        public async Task ThenCreateNewSurveyCode(Guid? uniqueSurveyCode, DateTime? inviteSentDate)
        {
            // Arrange
            var feedbackInvite = new FeedbackInvite 
            { 
                FeedbackId = FeedBackId, 
                UniqueSurveyCode = uniqueSurveyCode, 
                InviteSentDate = inviteSentDate 
            };

            _emailDetailsRepoMock
                .Setup(m => m.GetFeedbackInviteSentDateAsync(FeedBackId))
                .ReturnsAsync(feedbackInvite);

            // Act
            await _sut.GenerateSurveyInvites(_message);

            // Assert
            _emailDetailsRepoMock.Verify(mock => mock.InsertNewSurveyForFeedback(FeedBackId), Times.Once);
        }

        public static IEnumerable<object[]> CreateTestData 
        {
            get
            {
                yield return new object[] { null, null };
                yield return new object[] { Guid.NewGuid(), DateTime.Now.AddDays(InviteCycleDays * -1) };
            }
        }

        [Theory, MemberData(nameof(SkipCreationTestData))]
        public async Task ThenDontCreateNewSurveyCode(DateTime? inviteSentDate)
        {
            // Arrange
            var feedbackInvite = new FeedbackInvite 
            { 
                FeedbackId = FeedBackId, 
                UniqueSurveyCode = Guid.NewGuid(), 
                InviteSentDate = DateTime.Now.AddDays(InviteCycleDays - 1) 
            };

            _emailDetailsRepoMock
                .Setup(m => m.GetFeedbackInviteSentDateAsync(FeedBackId))
                .ReturnsAsync(feedbackInvite);

            // Act
            await _sut.GenerateSurveyInvites(_message);

            // Assert
            _emailDetailsRepoMock.Verify(mock => mock.InsertNewSurveyForFeedback(It.IsAny<long>()), Times.Never);
        }
        public static IEnumerable<object[]> SkipCreationTestData 
        {
            get
            {
                yield return new object[] { null };
                yield return new object[] { DateTime.Now.AddDays(InviteCycleDays - 1) };
            }
        }
    }
}
