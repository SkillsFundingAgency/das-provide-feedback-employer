using ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackResultAnnualQuery;
using ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackResultForAcademicYearQuery;

namespace UnitTests.Api
{
    using AutoFixture.Xunit2;
    using ESFA.DAS.EmployerProvideFeedback.Api.Controllers;
    using ESFA.DAS.EmployerProvideFeedback.Api.Models;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class EmployerFeedbackResultAPiTests
    {
        private readonly EmployerFeedbackResultController controller;

        private readonly Mock<ILogger<EmployerFeedbackResultController>> mockLogger;

        private readonly Mock<IMediator> mockMediator;


        public EmployerFeedbackResultAPiTests()
        {
            mockMediator = new Mock<IMediator>();
            mockLogger = new Mock<ILogger<EmployerFeedbackResultController>>();
            controller = new EmployerFeedbackResultController(mockMediator.Object, mockLogger.Object);
        }


        [Theory, AutoData]
        public async Task WhenGettingEmployerFeedbackResultAnnual_SendsFeedbackAnnualQuery(EmployerFeedbackAnnualResultDto feedback)
        {
            // arrange
            mockMediator.Setup(s => s.Send(It.IsAny<FeedbackResultAnnualQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(feedback);

            // act
            var actionResult = await this.controller.GetEmployerFeedbackResultAnnual(123);

            // assert
            var actionOkResult = actionResult as OkObjectResult;
            Assert.NotNull(actionOkResult);

            var model = actionOkResult.Value as EmployerFeedbackAnnualResultDto;
            Assert.NotNull(model);

            Assert.Equal(model.ReviewCount, feedback.ReviewCount);
        }

        [Theory, AutoData]
        public async Task WhenGettingEmployerFeedbackResultForAcademicYear_SendsFeedbackForAcademicYearQuery(EmployerFeedbackForAcademicYearResultDto feedback)
        {
            // arrange
            mockMediator.Setup(s => s.Send(It.IsAny<FeedbackResultForAcademicYearQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(feedback);

            // act
            var actionResult = await this.controller.GetEmployerFeedbackResultForAcademicYear(123,"AY2324");

            // assert
            var actionOkResult = actionResult as OkObjectResult;
            Assert.NotNull(actionOkResult);

            var model = actionOkResult.Value as EmployerFeedbackForAcademicYearResultDto;
            Assert.NotNull(model);

            Assert.Equal(model.ReviewCount, feedback.ReviewCount);
        }
    }
}