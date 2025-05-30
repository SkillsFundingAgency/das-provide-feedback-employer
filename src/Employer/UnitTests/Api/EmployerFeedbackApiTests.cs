using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using ESFA.DAS.EmployerProvideFeedback.Api.Controllers;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;
using ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackQuery;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace UnitTests.Api
{
    [TestFixture]
    public class FeedbackControllerTests
    {
        private FeedbackController _controller;
        private Mock<ILogger<FeedbackController>> _mockLogger;
        private Mock<IMediator> _mockMediator;

        [SetUp]
        public void SetUp()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<FeedbackController>>();
            _controller = new FeedbackController(_mockMediator.Object, _mockLogger.Object);
        }

        [Test, AutoData]
        public async Task WhenGettingAllFeedback_SendsFeedbackQuery(List<EmployerFeedbackDto> feedback)
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<FeedbackQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(feedback);

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result.As<OkObjectResult>();

            okResult.Value.Should().BeAssignableTo<List<EmployerFeedbackDto>>();
            var model = okResult.Value.As<List<EmployerFeedbackDto>>();

            model.Should().NotBeNull();
            model.Count.Should().Be(feedback.Count);
        }
    }
}