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
        [Fact]
        public async Task Valid_AccountId_Should_Return_View()
        {
            // Arrange
            var request = new GetProvidersForFeedbackRequest();
            var controller = GetProviderController();

            // Act
            await controller.Index(request);

            // Assert
            Assert.Single(controller.ViewData);
        }

        private Mock<ISessionService> GetMockSessionService()
        {
            var _surveyModel = new SurveyModel()
            {
                UserRef = Guid.NewGuid(),
                ProviderName = "TestProviderName",
            };

            var _sessionServiceMock = new Mock<ISessionService>();
            _sessionServiceMock.Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>())).Returns(Task.FromResult(_surveyModel));
            return _sessionServiceMock;
        }

        private Mock<ITrainingProviderService> GetMockTrainingProviderService()
        {
            var _trainingProviderServiceMock = new Mock<ITrainingProviderService>();
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

            return _trainingProviderServiceMock;
        }

        private Mock<IOptionsMonitor<MaPageConfiguration>> GetMaPageConfigurationMock()
        {
            var maPageConfiguration = new MaPageConfiguration
            {
                Routes = new MaRoutes
                {
                    Accounts = new Dictionary<string, string>()
                }
            };
            maPageConfiguration.Routes.Accounts.Add("AccountsHome", "http://AnAccountsLink/{0}");

            var mock = new Mock<IOptionsMonitor<MaPageConfiguration>>();
            mock.Setup(s => s.CurrentValue).Returns(maPageConfiguration);

            return mock;
        }

        private Mock<ILinkGenerator> GetMockLinkGenerator()
        {
            var mock = new Mock<ILinkGenerator>();
            mock.Setup(s => s.AccountsLink(It.IsAny<string>())).Returns<string>(x => x);
            return mock;
        }

        private static ControllerContext GetControllerContext()
        {
            var context = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "TestUserIdValue"),
                }))
            };

            return new ControllerContext
            {
                HttpContext = context
            };
        }

        private ProviderController GetProviderController()
        {
            var controller = new ProviderController(
               new Mock<IEmployerFeedbackRepository>().Object,
               GetMockSessionService().Object,
               GetMockTrainingProviderService().Object,
               new Mock<IEncodingService>().Object,
               new Mock<ILogger<ProviderController>>().Object,
               new UrlBuilder(new Mock<ILogger<UrlBuilder>>().Object, GetMaPageConfigurationMock().Object, GetMockLinkGenerator().Object));

            controller.ControllerContext = GetControllerContext();

            return controller;
        }
    }
}
