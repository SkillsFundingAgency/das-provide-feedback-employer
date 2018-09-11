using System;
using System.Collections.Generic;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.ProvideFeedback.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace UnitTests.EmployerProvideFeedback.Infrastructure
{
    public class EnsureFeedbackNotSubmittedAttributeTests
    {
        private readonly Mock<IStoreEmployerEmailDetails> _sessionServiceMock;
        private readonly Mock<Controller> _controllerMock;

        public EnsureFeedbackNotSubmittedAttributeTests()
        {
            _sessionServiceMock = new Mock<IStoreEmployerEmailDetails>();
            _controllerMock = new Mock<Controller>();
            _controllerMock.Setup(mock => mock.RedirectToRoute(It.IsAny<string>())).Returns(new RedirectToRouteResult(RouteNames.FeedbackAlreadySubmitted, null));
            _sessionServiceMock.Setup(mock => mock.IsCodeBurnt(It.IsAny<Guid>())).ReturnsAsync(true);
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

            var ensureSession = new EnsureFeedbackNotSubmitted(_sessionServiceMock.Object);

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
    }
}
