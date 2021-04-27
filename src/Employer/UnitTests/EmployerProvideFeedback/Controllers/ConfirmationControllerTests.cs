using System;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using ESFA.DAS.EmployerProvideFeedback.Configuration;
using ESFA.DAS.EmployerProvideFeedback.Controllers;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace UnitTests.EmployerProvideFeedback.Controllers
{
    public class ConfirmationControllerTests
    {
        private readonly ConfirmationController _controller;
        private readonly IFixture _fixture = new Fixture();
        private readonly SurveyModel _cachedSurveyModel;
        private ExternalLinksConfiguration _externalLinks = new ExternalLinksConfiguration
        {
            FindApprenticeshipTrainingSiteUrl = "findanapprentice.sfa.gov.uk"
        };

        public ConfirmationControllerTests()
        {
            _cachedSurveyModel = _fixture.Create<SurveyModel>();
            var sessionServiceMock = new Mock<ISessionService>();
            var loggerMock = new Mock<ILogger<ConfirmationController>>();
            var externalLinksOptions = Options.Create(_externalLinks);
            sessionServiceMock
                .Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>()))
                .Returns(Task.FromResult(_cachedSurveyModel));
            _controller = new ConfirmationController(
                sessionServiceMock.Object,
                externalLinksOptions,
                loggerMock.Object);
        }

        [Fact]
        public async void ApprenticeApi_ProviderHasFeedback_FeedbackDisplayed_InViewModel()
        {
            // Arrange
            var uniqueCode = Guid.NewGuid();

            // Act
            var result = await _controller.Index(uniqueCode) as ViewResult;

            // Assert
            var viewModel = Assert.IsAssignableFrom<ConfirmationViewModel>(result.Model);
            viewModel.FeedbackRating.Should().Be(_cachedSurveyModel.Rating);
            viewModel.ProviderName.Should().Be(_cachedSurveyModel.ProviderName);
            viewModel.FatUrl.ToLowerInvariant().Should().Be(_externalLinks.FindApprenticeshipTrainingSiteUrl.ToLowerInvariant());
        }

    }
}
