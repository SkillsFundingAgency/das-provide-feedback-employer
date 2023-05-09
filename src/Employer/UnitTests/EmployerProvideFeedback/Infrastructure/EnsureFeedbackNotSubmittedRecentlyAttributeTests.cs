using System;
using System.Collections.Generic;
using System.Security.Claims;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Controllers;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Encoding;
using Xunit;

namespace UnitTests.EmployerProvideFeedback.Infrastructure
{
    public class EnsureFeedbackNotSubmittedRecentlyAttributeTests
    {
        private readonly HomeController _controller;
        private readonly Mock<ISessionService> _sessionServiceMock;
        private readonly Mock<IEmployerFeedbackRepository> _employerEmailDetailsRepoMock;
        private readonly Mock<IEncodingService> _encodingServiceMock;
        private readonly Mock<ILogger<HomeController>> _loggerMock;

        public EnsureFeedbackNotSubmittedRecentlyAttributeTests()
        {
            _employerEmailDetailsRepoMock = new Mock<IEmployerFeedbackRepository>();
            _sessionServiceMock = new Mock<ISessionService>();
            _encodingServiceMock = new Mock<IEncodingService>();
            _loggerMock = new Mock<ILogger<HomeController>>();

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
                    new Claim(ClaimTypes.NameIdentifier, "TestUserIdValue"),
                }))
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
        }

        [Fact]
        public void When_Feedback_Submitted_Recently_Then_Redirect_To_FeedbackAlreadySubmitted()
        {
            // Arrange

            var config = new ProvideFeedbackEmployerWebConfiguration()
            {
                // Configure "recently" to be 10 days ago
                FeedbackWaitPeriodDays = 10
            };

            var sessionServiceMock = new Mock<IEmployerFeedbackRepository>();
            sessionServiceMock.Setup(mock => mock.IsCodeBurnt(It.IsAny<Guid>())).ReturnsAsync(true);
            // Set feedback given 5 days ago
            sessionServiceMock.Setup(mock => mock.GetCodeBurntDate(It.IsAny<Guid>())).ReturnsAsync(DateTime.Now.AddDays(-5));

            var httpContext = new DefaultHttpContext();
            var context = new ActionExecutingContext(
                new ActionContext
                {
                    HttpContext = httpContext,
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor()
                },
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
               _controller);
            context.ActionArguments.Add("uniqueCode", Guid.NewGuid());

            var ensureSession = new EnsureFeedbackNotSubmittedRecentlyAttribute(sessionServiceMock.Object, config);

            // Act
            ensureSession.OnActionExecuting(context);

            // Assert
            context
                .Result
                .Should()
                .NotBeNull()
                .And
                .BeAssignableTo<RedirectToRouteResult>()
                .Which
                .RouteName
                .Should()
                .BeEquivalentTo(RouteNames.FeedbackAlreadySubmitted);
        }

        [Fact]
        public void When_Feedback_Not_Submitted_Recently_Then_No_Redirect()
        {
            // Arrange

            var config = new ProvideFeedbackEmployerWebConfiguration()
            {
                // Configure "recently" to be 10 days ago
                FeedbackWaitPeriodDays = 10
            };

            var sessionServiceMock = new Mock<IEmployerFeedbackRepository>();
            sessionServiceMock.Setup(mock => mock.IsCodeBurnt(It.IsAny<Guid>())).ReturnsAsync(true);
            // Set feedback given 15 days ago
            sessionServiceMock.Setup(mock => mock.GetCodeBurntDate(It.IsAny<Guid>())).ReturnsAsync(DateTime.Now.AddDays(-15));

            var context = new ActionExecutingContext(
                new ActionContext
                {
                    HttpContext = _controller.HttpContext,
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor()
                },
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
               _controller);
            context.ActionArguments.Add("uniqueCode", Guid.NewGuid());

            var ensureSession = new EnsureFeedbackNotSubmittedRecentlyAttribute(sessionServiceMock.Object, config);

            // Act
            ensureSession.OnActionExecuting(context);

            // Assert
            context
                .Result
                .Should()
                .BeNull();
        }
    }
}
