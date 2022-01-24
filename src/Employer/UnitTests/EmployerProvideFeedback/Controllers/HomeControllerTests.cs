using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using ESFA.DAS.EmployerProvideFeedback.Controllers;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace UnitTests.EmployerProvideFeedback.Controllers
{
    public class HomeControllerTests
    {
        private readonly HomeController _controller;
        private readonly Mock<ISessionService> _sessionServiceMock;
        private readonly Mock<IEmployerFeedbackRepository> _employerEmailDetailsRepoMock;
        private readonly Mock<ILogger<HomeController>> _loggerMock; 
        private readonly List<ProviderAttributeModel> _providerAttributes;
        private IFixture _fixture = new Fixture();
        private readonly IOptions<List<ProviderAttributeModel>> _providerAttributeOptions;
        private readonly EmployerSurveyInvite _employerEmailDetail;

        public HomeControllerTests()
        {
            _employerEmailDetail = _fixture.Create<EmployerSurveyInvite>();
            _providerAttributes = GetProviderAttributes();
            _sessionServiceMock = new Mock<ISessionService>();
            _employerEmailDetailsRepoMock = new Mock<IEmployerFeedbackRepository>();
            _loggerMock = new Mock<ILogger<HomeController>>();
            _providerAttributeOptions = Options.Create(_providerAttributes);
            _controller = new HomeController(
                _employerEmailDetailsRepoMock.Object,
                _sessionServiceMock.Object,
                _loggerMock.Object);
            _employerEmailDetailsRepoMock.Setup(mock => mock.GetEmployerInviteForUniqueCode(It.IsAny<Guid>())).Returns(Task.FromResult(_employerEmailDetail));
        }

        [Fact]
        public async void UniqueCode_Invalid_ShouldRedirect_ToError()
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
        public async void SessionSurvey_DoesNotExist_ShouldPopulateProviderName_OnViewData_FromEmployerEmailDetail()
        {
            // Arrange
            var uniqueCode = Guid.NewGuid();

            // Act
            var result = await _controller.Index(uniqueCode) as ViewResult;

            // Assert
            var viewData = _controller.ViewData;
            Assert.Single(viewData);
            Assert.Equal(_employerEmailDetail.ProviderName, viewData["ProviderName"]);
        }

        [Fact]
        public async void SessionSurvey_DoesNotExist_ShouldCreateNewSurveyInSession()
        {
            // Arrange
            var uniqueCode = Guid.NewGuid();

            // Act
            var result = await _controller.Index(uniqueCode) as ViewResult;

            // Assert
            _sessionServiceMock.Verify(mock => mock.Set(uniqueCode.ToString(), It.IsAny<SurveyModel>()), Times.Once);
        }

        [Fact]
        public async  void SessionSurvey_Exists_ShouldNotCreateNewSurvey()
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
