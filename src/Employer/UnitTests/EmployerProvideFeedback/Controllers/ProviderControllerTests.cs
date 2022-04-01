using ESFA.DAS.EmployerProvideFeedback.Controllers;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.Paging;
using ESFA.DAS.EmployerProvideFeedback.Services;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Employer.Shared.UI.Configuration;
using SFA.DAS.EmployerUrlHelper;
using SFA.DAS.Encoding;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.EmployerProvideFeedback.Controllers
{
    public class ProviderControllerTests
    {
        private static readonly ProviderController _controller;
        private static readonly Mock<IEmployerFeedbackRepository> _employerEmailDetailsRepoMock;
        private static readonly Mock<ISessionService> _sessionServiceMock;
        private static readonly Mock<ITrainingProviderService> _trainingProviderServiceMock;
        private static readonly Mock<IEncodingService> _encodingServiceMock;
        private static readonly Mock<ILogger<ProviderController>> _loggerMock;
        private static readonly SurveyModel _surveyModel;
        private static readonly UrlBuilder _urlBuilder;
        private static readonly Mock<ILogger<UrlBuilder>> _urlBuilderloggerMock;
        private static readonly Mock<IOptionsMonitor<MaPageConfiguration>> _maPageConfigurationMock;
        private static readonly Mock<ILinkGenerator> _linkGeneratorMock;

        static ProviderControllerTests()
        {
            _surveyModel = new SurveyModel()
            {
                UserRef = Guid.NewGuid(),
                ProviderName = "TestProviderName",
            };

            _employerEmailDetailsRepoMock = new Mock<IEmployerFeedbackRepository>();

            _sessionServiceMock = new Mock<ISessionService>();
            _sessionServiceMock.Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>())).Returns(Task.FromResult(_surveyModel));

            _trainingProviderServiceMock = new Mock<ITrainingProviderService>();
            _trainingProviderServiceMock.Setup(m =>
                m.GetTrainingProviderSearchViewModel(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new ProviderSearchViewModel()
                {
                    TrainingProviders = new PaginatedList<ProviderSearchViewModel.EmployerTrainingProvider>(
                        new List<ProviderSearchViewModel.EmployerTrainingProvider>() { }, 0, 0, 0, 6)
                });

            _encodingServiceMock = new Mock<IEncodingService>();

            _loggerMock = new Mock<ILogger<ProviderController>>();

            _urlBuilderloggerMock = new Mock<ILogger<UrlBuilder>>();
            _maPageConfigurationMock = new Mock<IOptionsMonitor<MaPageConfiguration>>();
            _linkGeneratorMock = new Mock<ILinkGenerator>();
            var maPageConfiguration = new MaPageConfiguration
            {
                Routes = new MaRoutes
                {
                    Accounts = new Dictionary<string, string>()
                }
            };
            maPageConfiguration.Routes.Accounts.Add("AccountsHome", "http://AnAccountsLink/{0}");
            _maPageConfigurationMock.Setup(s => s.CurrentValue).Returns(maPageConfiguration);
            _linkGeneratorMock.Setup(s => s.AccountsLink(It.IsAny<string>())).Returns<string>(x => x);
            _urlBuilder = new UrlBuilder(_urlBuilderloggerMock.Object, _maPageConfigurationMock.Object, _linkGeneratorMock.Object);

            _controller = new ProviderController(
                _employerEmailDetailsRepoMock.Object,
                _sessionServiceMock.Object,
                _trainingProviderServiceMock.Object,
                _encodingServiceMock.Object,
                _loggerMock.Object,
                _urlBuilder);
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

        public class Index
        {
            [Fact]
            public async Task Valid_AccountId_Should_Return_View()
            {
                // Arrange
                var request = new GetProvidersForFeedbackRequest();

                // Act
                var result = await _controller.Index(request) as ViewResult;

                // Assert
                var viewData = _controller.ViewData;
                Assert.Single(viewData);
            }
        }
    }
}
