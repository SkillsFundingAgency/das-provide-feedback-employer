using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Das.ProvideFeedback.Domain.Entities;
using ESFA.DAS.EmployerProvideFeedback.Controllers;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data;
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
        private readonly Mock<IStoreEmployerEmailDetails> _employerEmailDetailsRepoMock;
        private readonly Mock<ILogger<HomeController>> _loggerMock; 
        private readonly List<ProviderAttributeModel> _providerAttributes;
        private IFixture _fixture = new Fixture();
        private readonly IOptions<List<ProviderAttributeModel>> _providerAttributeOptions;
        private readonly EmployerEmailDetail _employerEmailDetail;

        public HomeControllerTests()
        {
            _employerEmailDetail = _fixture.Create<EmployerEmailDetail>();
            _providerAttributes = GetProviderAttributes();
            _sessionServiceMock = new Mock<ISessionService>();
            _employerEmailDetailsRepoMock = new Mock<IStoreEmployerEmailDetails>();
            _loggerMock = new Mock<ILogger<HomeController>>();
            _providerAttributeOptions = Options.Create(_providerAttributes);
            _controller = new HomeController(
                _employerEmailDetailsRepoMock.Object,
                _sessionServiceMock.Object,
                _loggerMock.Object,
                _providerAttributeOptions);
            _employerEmailDetailsRepoMock.Setup(mock => mock.GetEmailDetailsForUniqueCode(It.IsAny<Guid>())).Returns(Task.FromResult(_employerEmailDetail));
        }

        [Fact]
        public async void UniqueCode_Invalid_ShouldRedirect_ToError()
        {
            // Arrange
            var uniqueCode = Guid.NewGuid();
            _employerEmailDetailsRepoMock.Setup(mock => mock.GetEmailDetailsForUniqueCode(uniqueCode)).ReturnsAsync(null as EmployerEmailDetail);

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
            _sessionServiceMock.Setup(mock => mock.Get<SurveyModel>(uniqueCode.ToString())).Returns(existingSurvey);

            // Act
            var result = await _controller.Index(uniqueCode) as ViewResult;

            // Assert
            _sessionServiceMock.Verify(mock => mock.Set(uniqueCode.ToString(), It.IsAny<SurveyModel>()), Times.Never);
        }

        private List<ProviderAttributeModel> GetProviderAttributes()
        {
            return _fixture
                .Build<ProviderAttributeModel>()
                .With(x => x.IsDoingWell, false)
                .With(x => x.IsToImprove, false)
                .CreateMany(10)
                .ToList();
        }
    }
}
