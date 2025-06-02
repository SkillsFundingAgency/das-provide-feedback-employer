using System;
using System.Collections.Generic;
using System.Security.Claims;
using ESFA.DAS.EmployerProvideFeedback.Authentication;
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
using NUnit.Framework;
using SFA.DAS.Encoding;

namespace UnitTests.EmployerProvideFeedback.Infrastructure
{
    public class EnsureSessionExistsAttributeTests
    {
        private readonly HomeController _controller;
        private readonly Mock<ISessionService> _sessionServiceMock;
        private readonly Mock<IEmployerFeedbackRepository> _employerEmailDetailsRepoMock;
        private readonly Mock<IEncodingService> _encodingServiceMock;
        private readonly Mock<ILogger<HomeController>> _controllerLoggerMock;
        private readonly Mock<ILogger<EnsureSessionExists>> _loggerMock;

        public EnsureSessionExistsAttributeTests()
        {
            _controllerLoggerMock = new Mock<ILogger<HomeController>>();
            _loggerMock = new Mock<ILogger<EnsureSessionExists>>();
            _employerEmailDetailsRepoMock = new Mock<IEmployerFeedbackRepository>();
            _sessionServiceMock = new Mock<ISessionService>();
            _encodingServiceMock = new Mock<IEncodingService>();

            _controller = new HomeController(
                            _employerEmailDetailsRepoMock.Object,
                            _sessionServiceMock.Object,
                            _encodingServiceMock.Object,
                            _controllerLoggerMock.Object,
                            null, 
                            null);
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

        [Test]
        public void Session_NotExists_Should_RedirectToLanding()
        {
            // Arrange
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

            var ensureSession = new EnsureSessionExists(_sessionServiceMock.Object, _loggerMock.Object);

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
                .BeEquivalentTo(RouteNames.Landing_Get);
        }
    }
}
