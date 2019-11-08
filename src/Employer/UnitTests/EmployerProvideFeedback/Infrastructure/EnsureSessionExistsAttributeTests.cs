﻿using System;
using System.Collections.Generic;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.EmployerProvideFeedback.Infrastructure
{
    public class EnsureSessionExistsAttributeTests
    {
        private readonly Mock<ISessionService> _sessionServiceMock;
        private readonly Mock<ILogger<EnsureSessionExists>> _loggerMock;
        private readonly Mock<Controller> _controllerMock;

        public EnsureSessionExistsAttributeTests()
        {
            _loggerMock = new Mock<ILogger<EnsureSessionExists>>();
            _sessionServiceMock = new Mock<ISessionService>();
            _controllerMock = new Mock<Controller>();
            _controllerMock.Setup(mock => mock.RedirectToRoute(It.IsAny<string>())).Returns(new RedirectToRouteResult(RouteNames.Landing_Get, null));
        }

        [Fact]
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
               _controllerMock.Object);
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
