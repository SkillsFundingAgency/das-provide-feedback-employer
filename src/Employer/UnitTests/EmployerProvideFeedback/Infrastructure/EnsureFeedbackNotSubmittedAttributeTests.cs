using System;
using System.Collections.Generic;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using SFA.DAS.Encoding;
using Xunit;

namespace UnitTests.EmployerProvideFeedback.Infrastructure
{
    public class EnsureFeedbackNotSubmittedAttributeTests
    {
        private readonly Mock<IEmployerFeedbackRepository> _sessionServiceMock;
        private readonly Mock<Controller> _controllerMock;
        private readonly Mock<IEncodingService> _encodingServiceMock;

        public EnsureFeedbackNotSubmittedAttributeTests()
        {
            _sessionServiceMock = new Mock<IEmployerFeedbackRepository>();
            _controllerMock = new Mock<Controller>();
            _controllerMock.Setup(mock => mock.RedirectToRoute(It.IsAny<string>(), It.IsAny<object>())).Returns(new RedirectToRouteResult(RouteNames.FeedbackAlreadySubmitted, new { encodedAccountId = "ABCDEF" }));
            _sessionServiceMock.Setup(mock => mock.IsCodeBurnt(It.IsAny<Guid>())).ReturnsAsync(true);
            _sessionServiceMock.Setup(mock => mock.GetEmployerInviteForUniqueCode(It.IsAny<Guid>())).ReturnsAsync(new EmployerSurveyInvite());
            _encodingServiceMock = new Mock<IEncodingService>();
            _encodingServiceMock.Setup(m => m.Encode(It.IsAny<long>(), EncodingType.AccountId)).Returns("ABCDEF");
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

            var ensureSession = new EnsureFeedbackNotSubmitted(_sessionServiceMock.Object, _encodingServiceMock.Object);

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
