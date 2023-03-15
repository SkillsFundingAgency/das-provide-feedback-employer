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
        private readonly HomeController _controller;
        private readonly Mock<ISessionService> _sessionServiceMock;
        private readonly Mock<IEmployerFeedbackRepository> _employerEmailDetailsRepoMock;
        private readonly Mock<IEncodingService> _encodingServiceMock;
        private readonly Mock<ILogger<HomeController>> _loggerMock;
        private IFixture _fixture = new Fixture();
        private readonly SurveyModel _surveyModel;
        private readonly int _mockAccountId;

        public HomeControllerTests()
        {
            _surveyModel = new SurveyModel()
            {
                UserRef = Guid.NewGuid()
            };
            _sessionServiceMock = new Mock<ISessionService>();
            _sessionServiceMock.Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>())).Returns(Task.FromResult(_surveyModel));
            _employerEmailDetailsRepoMock = new Mock<IEmployerFeedbackRepository>();
            _mockAccountId = new int();
            var context = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, _surveyModel.UserRef.ToString()),
                }))
            };
            _encodingServiceMock = new Mock<IEncodingService>();
            _loggerMock = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(
            _employerEmailDetailsRepoMock.Object,
            _sessionServiceMock.Object,
            _encodingServiceMock.Object,
            _loggerMock.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            _employerEmailDetailsRepoMock.Setup(mock => mock.GetEmployerAccountIdFromUniqueSurveyCode(It.IsAny<Guid>())).Returns(Task.FromResult(_mockAccountId));
        }

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
        public async void SessionSurvey_DoesNotExist_ShouldCreateNewSurveyInSession()
        {
            // Arrange
            var uniqueCode = Guid.NewGuid();
            var _controller = GetHomeController();
            var _employerEmailDetailsRepoMock = GetMockEmployerFeedbackRepository();
            _employerEmailDetailsRepoMock.Setup(mock => mock.GetEmployerAccountIdFromUniqueSurveyCode(uniqueCode)).ReturnsAsync(0);

            // Act
            var result = await _controller.Index(uniqueEmailCode) as ViewResult;

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
            return new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                    new Claim(ClaimTypes.NameIdentifier, GetSurveyModel().UserRef.ToString()),
                }))
            };
        }

        private SurveyModel GetSurveyModel() { return new SurveyModel() { UserRef = Guid.NewGuid() }; }

        private Mock<IEmployerFeedbackRepository> GetMockEmployerFeedbackRepository() { return new Mock<IEmployerFeedbackRepository>(); }

        private Mock<ISessionService> GetMockSessionService()
        {
            var _sessionServiceMock = new Mock<ISessionService>();
            _sessionServiceMock.Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>())).Returns(Task.FromResult(GetSurveyModel()));
            return _sessionServiceMock;
        }

        private Mock<IEncodingService> GetMockEncodingService() { return new Mock<IEncodingService>(); }

        private Mock<ILogger<HomeController>> GetMockLogger() { return new Mock<ILogger<HomeController>>(); }

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
