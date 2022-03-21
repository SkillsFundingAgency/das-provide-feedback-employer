namespace UnitTests.Api
{
    using AutoFixture.Xunit2;
    using ESFA.DAS.EmployerProvideFeedback.Api.Controllers;
    using ESFA.DAS.EmployerProvideFeedback.Api.Models;
    using ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackQuery;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class FeedbackControllerTests
    {
        private readonly FeedbackController controller;

        private readonly Mock<ILogger<FeedbackController>> mockLogger;

        private readonly Mock<IMediator> mockMediator;


        public FeedbackControllerTests()
        {
            mockMediator = new Mock<IMediator>();
            mockLogger = new Mock<ILogger<FeedbackController>>();
            controller = new FeedbackController(mockMediator.Object, mockLogger.Object);
        }


        [Theory, AutoData]
        public async Task WhenGettingAllFeedback_SendsFeedbackQuery(List<EmployerFeedbackDto> feedback)
        {
            // arrange
            mockMediator.Setup(s => s.Send(It.IsAny<FeedbackQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync(feedback);

            // act
            var actionResult = await this.controller.GetAll();

            // assert
            var actionOkResult = actionResult as OkObjectResult;
            Assert.NotNull(actionOkResult);

            var model = actionOkResult.Value as List<EmployerFeedbackDto>;
            Assert.NotNull(model);

            var actual = model.Count;

            Assert.Equal(feedback.Count, actual);
        }

    }
}