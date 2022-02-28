using System;
using System.Collections.Generic;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
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
    public class EnsureFeedbackNotSubmittedRecentlyAttributeTests
    {
        private readonly Mock<Controller> _controllerMock;

        public EnsureFeedbackNotSubmittedRecentlyAttributeTests()
        {
            _controllerMock = new Mock<Controller>();
            _controllerMock.Setup(mock => mock.RedirectToRoute(It.IsAny<string>())).Returns(new RedirectToRouteResult(RouteNames.FeedbackAlreadySubmitted, null));
            _controllerMock.Setup(mock => mock.View()).Returns(new ViewResult());
        }

        [Fact]
        public void When_Feedback_Submitted_Recently_Then_Redirect_To_FeedbackAlreadySubmitted()
        {
            // Arrange

            var config = new ProvideFeedbackEmployerWeb()
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
               _controllerMock.Object);
            context.ActionArguments.Add("uniqueCode", Guid.NewGuid());

            var ensureSession = new EnsureFeedbackNotSubmittedRecently(sessionServiceMock.Object, config);

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

            var config = new ProvideFeedbackEmployerWeb()
            {
                // Configure "recently" to be 10 days ago
                FeedbackWaitPeriodDays = 10
            };

            var sessionServiceMock = new Mock<IEmployerFeedbackRepository>();
            sessionServiceMock.Setup(mock => mock.IsCodeBurnt(It.IsAny<Guid>())).ReturnsAsync(true);
            // Set feedback given 15 days ago
            sessionServiceMock.Setup(mock => mock.GetCodeBurntDate(It.IsAny<Guid>())).ReturnsAsync(DateTime.Now.AddDays(-15));

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

            var ensureSession = new EnsureFeedbackNotSubmittedRecently(sessionServiceMock.Object, config);

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
