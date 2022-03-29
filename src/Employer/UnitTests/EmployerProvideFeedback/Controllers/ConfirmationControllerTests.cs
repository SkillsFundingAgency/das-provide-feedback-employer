using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using ESFA.DAS.EmployerProvideFeedback.Configuration;
using ESFA.DAS.EmployerProvideFeedback.Controllers;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Employer.Shared.UI.Configuration;
using SFA.DAS.EmployerUrlHelper;
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
            var optionsMock = new Mock<IOptionsMonitor<MaPageConfiguration>>();
            var linkGeneratorMock = new Mock<ILinkGenerator>();

            var maPageConfiguration = new MaPageConfiguration
            {
                Routes = new MaRoutes
                {
                    Accounts = new Dictionary<string, string>()
                }
            };
            maPageConfiguration.Routes.Accounts.Add("AccountsHome", "http://AnAccountsLink/{0}");
            optionsMock.Setup(s => s.CurrentValue).Returns(maPageConfiguration);
            linkGeneratorMock.Setup(s => s.AccountsLink(It.IsAny<string>())).Returns<string>(x => x);

            var config = new ProvideFeedbackEmployerWebConfiguration()
            {
                ExternalLinks = _externalLinks
            };
            var urlBuilder = new UrlBuilder(Mock.Of<ILogger<UrlBuilder>>(), optionsMock.Object, linkGeneratorMock.Object);
            sessionServiceMock
                .Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>()))
                    .Returns(Task.FromResult(_cachedSurveyModel));
            _controller = new ConfirmationController(
                sessionServiceMock.Object,
                config,
                urlBuilder,
                loggerMock.Object);

            var context = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "TestUserIdValue"),
                }))
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
        }

        [Fact]
        public async void ApprenticeApi_ProviderHasFeedback_FeedbackDisplayed_InViewModel()
        {
            // Arrange
            var encodedAccountId = "ABCDEFG";

            // Act
            var result = await _controller.Index(encodedAccountId) as ViewResult;

            // Assert
            var viewModel = Assert.IsAssignableFrom<ConfirmationViewModel>(result.Model);
            viewModel.FeedbackRating.Should().Be(_cachedSurveyModel.Rating);
            viewModel.ProviderName.Should().Be(_cachedSurveyModel.ProviderName);
            viewModel.FatUrl.ToLowerInvariant().Should().Be(_externalLinks.FindApprenticeshipTrainingSiteUrl.ToLowerInvariant());
            viewModel.EmployerAccountsHomeUrl.Should().Be($"http://AnAccountsLink/{encodedAccountId}");
        }

    }
}
