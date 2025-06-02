using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;
using ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackResultQuery;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace UnitTests.Api
{
    public class FeedbackResultQueryTests
    {
        private readonly Mock<ILogger<FeedbackResultQueryHandler>> mockLogger;
        private readonly Mock<IEmployerFeedbackRepository> mockRepository;
        private readonly FeedbackResultQueryHandler handler;

        public FeedbackResultQueryTests()
        {
            mockLogger = new Mock<ILogger<FeedbackResultQueryHandler>>();
            mockRepository = new Mock<IEmployerFeedbackRepository>();
            handler = new FeedbackResultQueryHandler(mockRepository.Object, mockLogger.Object);
        }

        [Test]
        public async Task WhenQueryingFeedbackResult_IfNullReturnsEmptyCollection()
        {
            // Arrange
            mockRepository
                .Setup(s => s.GetFeedbackResultSummary(123))
                .ReturnsAsync((IEnumerable<EmployerFeedbackResultSummary>)null);

            // Act
            var response = await handler.Handle(new FeedbackResultQuery { Ukprn = 123 }, new CancellationToken());

            // Assert
            response.Ukprn.Should().Be(123);
            response.ReviewCount.Should().Be(0);
            response.Stars.Should().Be(0);
            response.ProviderAttribute.Should().BeAssignableTo<IEnumerable<ProviderAttributeSummaryItemDto>>();
            response.ProviderAttribute.Should().BeEmpty();
        }

        [Test]
        public async Task WhenQueryingFeedbackResult_IfNoFeedbackReturnsEmptyCollection()
        {
            // Arrange
            mockRepository
                .Setup(s => s.GetFeedbackResultSummary(456))
                .ReturnsAsync(new List<EmployerFeedbackResultSummary>());

            // Act
            var response = await handler.Handle(new FeedbackResultQuery { Ukprn = 456 }, new CancellationToken());

            // Assert
            response.Ukprn.Should().Be(456);
            response.ReviewCount.Should().Be(0);
            response.Stars.Should().Be(0);
            response.ProviderAttribute.Should().BeAssignableTo<IEnumerable<ProviderAttributeSummaryItemDto>>();
            response.ProviderAttribute.Should().BeEmpty(); // corrected to match empty list
        }

        [Test]
        public async Task WhenQueryingFeedbackResult_IfFeedbackExistsReturnsConvertedModel()
        {
            // Arrange
            var summaries = new Fixture().CreateMany<EmployerFeedbackResultSummary>(1).ToList();
            summaries[0].Ukprn = 789;
            mockRepository
                .Setup(s => s.GetFeedbackResultSummary(789))
                .ReturnsAsync(summaries);

            // Act
            var response = await handler.Handle(new FeedbackResultQuery { Ukprn = 789 }, new CancellationToken());

            // Assert
            var summary = summaries[0];
            response.Ukprn.Should().Be(summary.Ukprn);
            response.ReviewCount.Should().Be(summary.ReviewCount);
            response.Stars.Should().Be(summary.Stars);
            response.ProviderAttribute.Should().BeAssignableTo<IEnumerable<ProviderAttributeSummaryItemDto>>();
            response.ProviderAttribute.Should().NotBeEmpty();
        }
    }
}
