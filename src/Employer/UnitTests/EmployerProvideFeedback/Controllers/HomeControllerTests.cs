using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using ESFA.DAS.EmployerProvideFeedback.Authentication;
using ESFA.DAS.EmployerProvideFeedback.Controllers;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly List<ProviderAttributeModel> _providerAttributes;
        private IFixture _fixture = new Fixture();
        private readonly IOptions<List<ProviderAttributeModel>> _providerAttributeOptions;
        private readonly EmployerSurveyInvite _employerEmailDetail;
        private readonly SurveyModel _surveyModel;

        public HomeControllerTests()
        {
            _employerEmailDetail = _fixture.Create<EmployerSurveyInvite>();
            _surveyModel = new SurveyModel()
            {
                UserRef = Guid.NewGuid(),
                ProviderName = _employerEmailDetail.ProviderName,
            };
            _providerAttributes = GetProviderAttributes();
            _sessionServiceMock = new Mock<ISessionService>();
            _sessionServiceMock.Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>())).Returns(Task.FromResult(_surveyModel));
            _employerEmailDetailsRepoMock = new Mock<IEmployerFeedbackRepository>();
            _encodingServiceMock = new Mock<IEncodingService>();
            _loggerMock = new Mock<ILogger<HomeController>>();
            _providerAttributeOptions = Options.Create(_providerAttributes);
            _controller = new HomeController(
                _employerEmailDetailsRepoMock.Object,
                _sessionServiceMock.Object,
                _encodingServiceMock.Object,
                _loggerMock.Object,
                null,
                null);
            var context = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(EmployerClaims.UserId, _surveyModel.UserRef.ToString()),
                }))
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            _employerEmailDetailsRepoMock.Setup(mock => mock.GetEmployerInviteForUniqueCode(It.IsAny<Guid>())).Returns(Task.FromResult(_employerEmailDetail));
        }

        [Fact]
        public async Task UniqueCode_Invalid_ShouldRedirect_ToError()
        {
            // Arrange
            var uniqueCode = Guid.NewGuid();
            _employerEmailDetailsRepoMock.Setup(mock => mock.GetEmployerInviteForUniqueCode(uniqueCode)).ReturnsAsync(null as EmployerSurveyInvite);

            // Act
            var result = await _controller.Index(uniqueCode);

            // Assert
            Assert.IsAssignableFrom<NotFoundResult>(result);
        }

        [Fact]
        public async Task SessionSurvey_DoesNotExist_ShouldPopulateProviderName_OnViewData_FromEmployerEmailDetail()
        {
            // Arrange
            var request = new StartFeedbackRequest();

            // Act
            var result = await _controller.Index(request) as ViewResult;

            // Assert
            var viewData = _controller.ViewData;
            Assert.Single(viewData);
            Assert.Equal(_employerEmailDetail.ProviderName, viewData["ProviderName"]);
        }

        [Fact]
        public async Task SessionSurvey_DoesNotExist_ShouldCreateNewSurveyInSession()
        {
            // Arrange
            var uniqueEmailCode = Guid.NewGuid();

            // Act
            var result = await _controller.Index(uniqueEmailCode) as ViewResult;

            // Assert
            _sessionServiceMock.Verify(mock => mock.Set(_surveyModel.UserRef.ToString(), It.IsAny<SurveyModel>()), Times.Once);
        }

        [Fact]
        public async Task EmailEntryPoint_Should_Create_AccountId()
        {
            // Arrange
            var uniqueCode = Guid.NewGuid();

            // Act
            var result = await _controller.Index(uniqueCode) as ViewResult;

            // Assert
            _employerEmailDetailsRepoMock.Verify(mock => mock.GetEmployerInviteForUniqueCode(uniqueCode), Times.Once);
        }

        [Fact]
        public async Task SessionSurvey_Exists_ShouldNotCreateNewSurvey()
        {
            // Arrange
            var existingSurvey = _fixture.Create<SurveyModel>();
            var uniqueCode = Guid.NewGuid();
            _sessionServiceMock.Setup(mock => mock.Get<SurveyModel>(uniqueCode.ToString())).ReturnsAsync(existingSurvey);

            // Act
            var result = await _controller.Index(uniqueCode) as ViewResult;

            // Assert
            _sessionServiceMock.Verify(mock => mock.Set(uniqueCode.ToString(), It.IsAny<SurveyModel>()), Times.Never);
        }

        private List<ProviderAttributeModel> GetProviderAttributes()
        {
            return _fixture
                .Build<ProviderAttributeModel>()
                .With(x => x.Good, false)
                .With(x => x.Bad, false)
                .CreateMany(10)
                .ToList();
        }
    }
}
