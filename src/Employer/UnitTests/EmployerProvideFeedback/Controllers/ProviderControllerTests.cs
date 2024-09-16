using ESFA.DAS.EmployerProvideFeedback.Controllers;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.Paging;
using ESFA.DAS.EmployerProvideFeedback.Services;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Encoding;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Authentication;
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

            _urlBuilder = new UrlBuilder("LOCAL");

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
                    new Claim(EmployerClaims.UserId, "TestUserIdValue"),
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
