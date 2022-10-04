﻿using AutoFixture.Xunit2;
using ESFA.DAS.EmployerProvideFeedback.Orchestrators;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace UnitTests.Orchestrators
{
    public class ReviewAnswersOrchestratorTests
    {
        private readonly ReviewAnswersOrchestrator orchestrator;
        private readonly Mock<IEmployerFeedbackRepository> employerFeedbackRepository;
        private readonly Mock<ILogger<ReviewAnswersOrchestrator>> logger;

        private readonly Mock<EmployerFeedback> employerFeedback;

        public ReviewAnswersOrchestratorTests()
        {
            employerFeedbackRepository = new Mock<IEmployerFeedbackRepository>();
            logger = new Mock<ILogger<ReviewAnswersOrchestrator>>();
            orchestrator = new ReviewAnswersOrchestrator(employerFeedbackRepository.Object, logger.Object);

            employerFeedback = new Mock<EmployerFeedback>();
        }

        [Theory, AutoData]
        public async void WhenUsingEmailJourney_ThenBurnDateIsSet(SurveyModel surveyModel)
        {
            //Arrange
            employerFeedback.Object.FeedbackId = 1;
            employerFeedbackRepository.Setup(x => x.GetEmployerFeedbackRecord(surveyModel.UserRef, surveyModel.AccountId, surveyModel.Ukprn))
                .ReturnsAsync(employerFeedback.Object);
           
            //Act
            await orchestrator.SubmitConfirmedEmployerFeedback(surveyModel);

            //Assert
            employerFeedbackRepository.Verify(x => x.SetCodeBurntDate(It.IsAny<Guid>()), Times.Once);
            employerFeedbackRepository.Verify(x => x.GetUniqueSurveyCodeFromFeedbackId(It.IsAny<long>()), Times.Never);
        }

        [Theory, AutoData]
        public async void WhenUsingAdHocJourney_ThenBurnDateIsSet(SurveyModel surveyModel)
        {
            //Arrange
            surveyModel.UniqueCode = null;

            employerFeedback.Object.FeedbackId = 1;
            employerFeedbackRepository.Setup(x => x.GetEmployerFeedbackRecord(surveyModel.UserRef, surveyModel.AccountId, surveyModel.Ukprn))
                .ReturnsAsync(employerFeedback.Object);

            //Act
            await orchestrator.SubmitConfirmedEmployerFeedback(surveyModel);

            //Assert
            employerFeedbackRepository.Verify(x => x.SetCodeBurntDate(It.IsAny<Guid>()), Times.Once);
            employerFeedbackRepository.Verify(x => x.GetUniqueSurveyCodeFromFeedbackId(It.IsAny<long>()), Times.Once);
        }
    }
}
