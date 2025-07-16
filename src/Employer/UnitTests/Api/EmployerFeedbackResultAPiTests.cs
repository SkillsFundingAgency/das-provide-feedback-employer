using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using ESFA.DAS.EmployerProvideFeedback.Api.Controllers;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;
using ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackResultAnnualQuery;
using ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackResultForAcademicYearQuery;
using ESFA.DAS.EmployerProvideFeedback.Api.Queries.ProviderSummaryStarsQuery;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace UnitTests.Api
{
    public class EmployerFeedbackResultApiTests
    {
        private EmployerFeedbackResultController _controller;
        private Mock<ILogger<EmployerFeedbackResultController>> _mockLogger;
        private Mock<IMediator> _mockMediator;

        [SetUp]
        public void SetUp()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<EmployerFeedbackResultController>>();
            _controller = new EmployerFeedbackResultController(_mockMediator.Object, _mockLogger.Object);
        }

        [Test, AutoData]
        public async Task WhenGettingEmployerFeedbackResultAnnual_SendsFeedbackAnnualQuery(EmployerFeedbackAnnualResultDto feedback)
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<FeedbackResultAnnualQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(feedback);

            // Act
            var result = await _controller.GetEmployerFeedbackResultAnnual(123);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result.As<OkObjectResult>();

            okResult.Value.Should().BeOfType<EmployerFeedbackAnnualResultDto>();
            var model = okResult.Value.As<EmployerFeedbackAnnualResultDto>();

            model.AnnualEmployerFeedbackDetails.Should().HaveSameCount(feedback.AnnualEmployerFeedbackDetails);

            if (model.AnnualEmployerFeedbackDetails.Any())
            {
                var expected = feedback.AnnualEmployerFeedbackDetails.First();
                var actual = model.AnnualEmployerFeedbackDetails.First();

                actual.Ukprn.Should().Be(expected.Ukprn);
                actual.TimePeriod.Should().Be(expected.TimePeriod);
                actual.Stars.Should().Be(expected.Stars);
                actual.ReviewCount.Should().Be(expected.ReviewCount);
                actual.ProviderAttribute.Should().HaveSameCount(expected.ProviderAttribute);

                if (expected.ProviderAttribute.Any())
                {
                    var expectedAttr = expected.ProviderAttribute.First();
                    var actualAttr = actual.ProviderAttribute.First();

                    actualAttr.Name.Should().Be(expectedAttr.Name);
                    actualAttr.Strength.Should().Be(expectedAttr.Strength);
                    actualAttr.Weakness.Should().Be(expectedAttr.Weakness);
                }
            }
        }

        [Test, AutoData]
        public async Task WhenGettingEmployerFeedbackResultForAcademicYear_SendsFeedbackForAcademicYearQuery(EmployerFeedbackForAcademicYearResultDto feedback)
        {
            // Arange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<FeedbackResultForAcademicYearQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(feedback);

            // Act
            var result = await _controller.GetEmployerFeedbackResultForAcademicYear(123, "AY2324");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result.As<OkObjectResult>();

            okResult.Value.Should().BeOfType<EmployerFeedbackForAcademicYearResultDto>();
            var model = okResult.Value.As<EmployerFeedbackForAcademicYearResultDto>();

            model.ReviewCount.Should().Be(feedback.ReviewCount);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("All")]
        [TestCase("AY2024")]
        [TestCase("AY1234")]
        public async Task GetAllStarsSummary_WithValidTimePeriod_ReturnsOk(string timePeriod)
        {
            var expected = new[] { new EmployerFeedbackForStarsSummaryDto { Ukprn = 1, ReviewCount = 5, Stars = 4} };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<ProviderSummaryStarsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetAllStarsSummary(timePeriod);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(expected);
        }
    }
}
