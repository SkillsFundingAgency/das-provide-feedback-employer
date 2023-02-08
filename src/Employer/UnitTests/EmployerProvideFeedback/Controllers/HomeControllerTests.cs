using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using ESFA.DAS.EmployerProvideFeedback.Controllers;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Encoding;
using Xunit;

namespace UnitTests.EmployerProvideFeedback.Controllers
{
    public class HomeControllerTests
    {
        [Fact]
        public async void SessionSurvey_DoesNotExist_ShouldPopulateProviderName_OnViewData_FromEmployerEmailDetail()
        {
            // Arrange
            var _controller = GetHomeController();
            var request = new StartFeedbackRequest();

            // Act
            var result = await _controller.Index(request) as ViewResult;

            // Assert
            var viewData = _controller.ViewData;
            Assert.Single(viewData);
            Assert.Equal(result.ViewData["ProviderName"], viewData["ProviderName"]);
        }

        [Fact]
        public async void UniqueCode_Invalid_ShouldRedirect_ToError()
        {
            // Arrange
            var uniqueCode = Guid.NewGuid();
            var _controller = GetHomeController();
            var _employerEmailDetailsRepoMock = GetMockEmployerFeedbackRepository();
            _employerEmailDetailsRepoMock.Setup(mock => mock.GetEmployerAccountIdFromUniqueSurveyCode(uniqueCode)).ReturnsAsync(0);

            // Act
            var result = await _controller.Index(uniqueCode);

            // Assert
            Assert.IsAssignableFrom<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(99)]
        [InlineData(int.MaxValue)]
        public async void EmailEntryPoint_Should_Get_AccountId(int _mockAccountId)
        {
            // Arrange
            var uniqueCode = Guid.NewGuid();

            var _mockEmployerFeedbackRepository = GetMockEmployerFeedbackRepository();
            _mockEmployerFeedbackRepository.Setup(mock => mock.GetEmployerAccountIdFromUniqueSurveyCode(It.IsAny<Guid>())).Returns(Task.FromResult(_mockAccountId));

            var _controller = new HomeController(
                _mockEmployerFeedbackRepository.Object,
                GetMockSessionService().Object,
                GetMockEncodingService().Object,
                GetMockLogger().Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = GetHttpContext(),
            };

            // Act
            var result = await _controller.Index(uniqueCode) as ViewResult;

            // Assert
            _mockEmployerFeedbackRepository.Verify(mock => mock.GetEmployerAccountIdFromUniqueSurveyCode(uniqueCode), Times.Once);
        }

        [Fact]
        public async void SessionSurvey_Exists_ShouldNotCreateNewSurvey()
        {
            // Arrange
            var _fixture = new Fixture();
            var existingSurvey = _fixture.Create<SurveyModel>();
            var uniqueCode = Guid.NewGuid();
            var _sessionServiceMock = GetMockSessionService();
            var _controller = GetHomeController();
            _sessionServiceMock.Setup(mock => mock.Get<SurveyModel>(uniqueCode.ToString())).ReturnsAsync(existingSurvey);

            // Act
            var result = await _controller.Index(uniqueCode) as ViewResult;

            // Assert
            _sessionServiceMock.Verify(mock => mock.Set(uniqueCode.ToString(), It.IsAny<SurveyModel>()), Times.Never);
        }

        private DefaultHttpContext GetHttpContext()
        {
            var _context = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, GetSurveyModel().UserRef.ToString()),
                }))
            };
            return _context;
        }

        private SurveyModel GetSurveyModel()
        {
            var _surveyModel = new SurveyModel()
            {
                UserRef = Guid.NewGuid()
            };
            return _surveyModel;
        }

        private Mock<IEmployerFeedbackRepository> GetMockEmployerFeedbackRepository()
        {
            var _employerEmailDetailsRepoMock = new Mock<IEmployerFeedbackRepository>();
            return _employerEmailDetailsRepoMock;
        }

        private Mock<ISessionService> GetMockSessionService()
        {
            var _sessionServiceMock = new Mock<ISessionService>();
            _sessionServiceMock.Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>())).Returns(Task.FromResult(GetSurveyModel()));
            return _sessionServiceMock;
        }

        private Mock<IEncodingService> GetMockEncodingService()
        {
            var _encodingServiceMock = new Mock<IEncodingService>();
            return _encodingServiceMock;
        }

        private Mock<ILogger<HomeController>> GetMockLogger()
        {
            var _loggerMock = new Mock<ILogger<HomeController>>();
            return _loggerMock;
        }

        private HomeController GetHomeController()
        {
            var _controller = new HomeController(
            GetMockEmployerFeedbackRepository().Object,
            GetMockSessionService().Object,
            GetMockEncodingService().Object,
            GetMockLogger().Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = GetHttpContext(),
            };
            return _controller;
        }
    }
}
