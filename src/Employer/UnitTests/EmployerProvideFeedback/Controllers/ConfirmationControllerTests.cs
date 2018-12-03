using System;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using ESFA.DAS.EmployerProvideFeedback.Configuration;
using ESFA.DAS.EmployerProvideFeedback.Controllers;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.Apprenticeships.Api.Types.Providers;
using Xunit;

namespace UnitTests.EmployerProvideFeedback.Controllers
{
    public class ConfirmationControllerTests
    {
        private readonly ConfirmationController _controller;
        private readonly Mock<ISessionService> _sessionServiceMock;
        private readonly IOptions<ExternalLinksConfiguration> _externalLinksOptions;
        private readonly Mock<IGetProviderFeedback> _providerFeedbackRepoMock;
        private readonly Mock<ILogger<ConfirmationController>> _loggerMock;
        private IFixture Fixture = new Fixture();
        private readonly SurveyModel _cachedSurveyModel;
        private ExternalLinksConfiguration _externalLinks = new ExternalLinksConfiguration
        {
            FindApprenticeshipTrainingSiteUrl = "findanapprentice.sfa.gov.uk"
        };

        public ConfirmationControllerTests()
        {
            _cachedSurveyModel = Fixture.Create<SurveyModel>();
            _sessionServiceMock = new Mock<ISessionService>();
            _providerFeedbackRepoMock = new Mock<IGetProviderFeedback>();
            _loggerMock = new Mock<ILogger<ConfirmationController>>();
            _externalLinksOptions = Options.Create(_externalLinks);
            _sessionServiceMock.Setup(mock => mock.GetAsync<SurveyModel>(It.IsAny<string>())).Returns(Task.FromResult(_cachedSurveyModel));
            _controller = new ConfirmationController(
                _sessionServiceMock.Object,
                _providerFeedbackRepoMock.Object,
                _externalLinksOptions,
                _loggerMock.Object);
        }

        [Fact]
        public async void ApprenticeApi_ProviderHasFeedback_FeedbackDisplayed_InViewModel()
        {
            // Arrange
            var uniqueCode = Guid.NewGuid();
            var feedback = Fixture.Create<Feedback>();
            _providerFeedbackRepoMock.Setup(mock => mock.GetProviderFeedbackAsync(It.IsAny<long>())).Returns(Task.FromResult(feedback));

            // Act
            var result = await _controller.Index(uniqueCode) as ViewResult;

            // Assert
            var viewModel = Assert.IsAssignableFrom<ConfirmationViewModel>(result.Model);
            Assert.NotNull(viewModel.Feedback);
            viewModel.FeedbackRating.Should().Be(_cachedSurveyModel.Rating);
            viewModel.ProviderName.Should().Be(_cachedSurveyModel.ProviderName);
            viewModel.FatProviderSearch.ToLowerInvariant().Should().Be(Path.Combine(_externalLinks.FindApprenticeshipTrainingSiteUrl, "provider", "search").ToLowerInvariant());
        }

        [Fact]
        public async void ApprenticeApi_ProviderHasNoPreviousFeedback_NoFeedbackDisplayed_InViewModel()
        {
            // Arrange
            var uniqueCode = Guid.NewGuid();
            _providerFeedbackRepoMock.Setup(mock => mock.GetProviderFeedbackAsync(It.IsAny<long>())).Returns(Task.FromResult(null as Feedback));

            // Act
            var result = await _controller.Index(uniqueCode) as ViewResult;

            // Assert
            var viewModel = Assert.IsAssignableFrom<ConfirmationViewModel>(result.Model);
            Assert.Null(viewModel.Feedback);
        }

        [Fact]
        public async void ApprenticeApi_ReturnsError_ViewShouldReturn_NullFeedback()
        {
            // Arrange
            var uniqueCode = Guid.NewGuid();

            _providerFeedbackRepoMock.Setup(mock => mock.GetProviderFeedbackAsync(It.IsAny<long>())).Throws(new Exception());

            // Act
            var result = await _controller.Index(uniqueCode) as ViewResult;

            // Assert
            var viewModel = Assert.IsAssignableFrom<ConfirmationViewModel>(result.Model);
            Assert.Null(viewModel.Feedback);
        }
    }
}
