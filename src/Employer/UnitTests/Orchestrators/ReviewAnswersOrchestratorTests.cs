using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using ESFA.DAS.EmployerProvideFeedback.Orchestrators;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace UnitTests.Orchestrators
{
    [TestFixture]
    public class ReviewAnswersOrchestratorTests
    {
        private ReviewAnswersOrchestrator _orchestrator;
        private Mock<IEmployerFeedbackRepository> _employerFeedbackRepository;
        private Mock<ILogger<ReviewAnswersOrchestrator>> _logger;
        private Mock<EmployerFeedback> _employerFeedback;

        [SetUp]
        public void SetUp()
        {
            _employerFeedbackRepository = new Mock<IEmployerFeedbackRepository>();
            _logger = new Mock<ILogger<ReviewAnswersOrchestrator>>();
            _orchestrator = new ReviewAnswersOrchestrator(_employerFeedbackRepository.Object, _logger.Object);

            _employerFeedback = new Mock<EmployerFeedback>();
        }

        [Test, AutoData]
        public async Task WhenUsingEmailJourney_ThenBurnDateIsSet(SurveyModel surveyModel)
        {
            _employerFeedback.Object.FeedbackId = 1;
            _employerFeedbackRepository.Setup(x =>
                x.GetEmployerFeedbackRecord(surveyModel.UserRef, surveyModel.AccountId, surveyModel.Ukprn))
                .ReturnsAsync(_employerFeedback.Object);

            await _orchestrator.SubmitConfirmedEmployerFeedback(surveyModel);

            _employerFeedbackRepository.Verify(x => x.SetCodeBurntDate(It.IsAny<Guid>()), Times.Once);
            _employerFeedbackRepository.Verify(x => x.GetUniqueSurveyCodeFromFeedbackId(It.IsAny<long>()), Times.Never);
        }

        [Test, AutoData]
        public async Task WhenUsingAdHocJourney_ThenBurnDateIsSet(SurveyModel surveyModel)
        {
            // Arrange
            surveyModel.UniqueCode = null;
            _employerFeedback.Object.FeedbackId = 1;

            _employerFeedbackRepository.Setup(x =>
                x.GetEmployerFeedbackRecord(surveyModel.UserRef, surveyModel.AccountId, surveyModel.Ukprn))
                .ReturnsAsync(_employerFeedback.Object);

            _employerFeedbackRepository.Setup(x =>
                x.GetUniqueSurveyCodeFromFeedbackId(_employerFeedback.Object.FeedbackId))
                .ReturnsAsync(Guid.NewGuid());

            // Act
            await _orchestrator.SubmitConfirmedEmployerFeedback(surveyModel);

            // Assert
            _employerFeedbackRepository.Verify(x => x.SetCodeBurntDate(It.IsAny<Guid>()), Times.Once);
            _employerFeedbackRepository.Verify(x => x.GetUniqueSurveyCodeFromFeedbackId(It.IsAny<long>()), Times.Once);
        }

        [Test, AutoData]
        public async Task WhenUsingAdHocJourney_AndNoSurveyInvite_NoBurnDateSet(SurveyModel surveyModel)
        {
            // Arrange
            surveyModel.UniqueCode = null;
            _employerFeedback.Object.FeedbackId = 1;

            _employerFeedbackRepository.Setup(x =>
                x.GetEmployerFeedbackRecord(surveyModel.UserRef, surveyModel.AccountId, surveyModel.Ukprn))
                .ReturnsAsync(_employerFeedback.Object);

            _employerFeedbackRepository.Setup(x =>
                x.GetUniqueSurveyCodeFromFeedbackId(_employerFeedback.Object.FeedbackId))
                .ReturnsAsync(Guid.Empty);

            // Act
            await _orchestrator.SubmitConfirmedEmployerFeedback(surveyModel);

            // Assert
            _employerFeedbackRepository.Verify(x => x.SetCodeBurntDate(It.IsAny<Guid>()), Times.Never);
            _employerFeedbackRepository.Verify(x => x.GetUniqueSurveyCodeFromFeedbackId(It.IsAny<long>()), Times.Once);
        }
    }
}
