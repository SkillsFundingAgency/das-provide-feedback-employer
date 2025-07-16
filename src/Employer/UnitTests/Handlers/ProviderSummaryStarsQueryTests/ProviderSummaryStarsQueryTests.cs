using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;
using ESFA.DAS.EmployerProvideFeedback.Api.Queries.ProviderSummaryStarsQuery;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace UnitTests.Api
{
    public class ProviderSummaryStarsQueryTests
    {
        private readonly Mock<ILogger<ProviderSummaryStarsQueryHandler>> mockLogger;
        private readonly Mock<IEmployerFeedbackRepository> mockRepository;
        private readonly ProviderSummaryStarsQueryHandler handler;

        public ProviderSummaryStarsQueryTests()
        {
            mockLogger = new Mock<ILogger<ProviderSummaryStarsQueryHandler>>();
            mockRepository = new Mock<IEmployerFeedbackRepository>();
            handler = new ProviderSummaryStarsQueryHandler(mockRepository.Object, mockLogger.Object);
        }

        [Test]
        public async Task WhenQueryingProviderSummaryStars_IfNullReturnsNull()
        {
            // Arrange
            mockRepository
                .Setup(s => s.GetAllStarsSummary(It.IsAny<string>()))
                .ReturnsAsync((IEnumerable<ProviderStarsSummary>)null);

            // Act
            var response = await handler.Handle(new ProviderSummaryStarsQuery(), new CancellationToken());

            // Assert
            response.Should().BeNull();
        }

        [Test]
        public async Task WhenQueryingProviderSummaryStars_IfNoSummaryStarsReturnsEmptyCollection()
        {
            // Arrange
            mockRepository
                .Setup(s => s.GetAllStarsSummary(It.IsAny<string>()))
                .ReturnsAsync(new List<ProviderStarsSummary>());

            // Act
            var response = await handler.Handle(new ProviderSummaryStarsQuery(), new CancellationToken());

            // Assert
            response.Should().BeAssignableTo<IEnumerable<EmployerFeedbackStarsSummaryDto>>();
            response.Should().BeEmpty();
        }

        [Test]
        public async Task WhenQueryingProviderSummaryStars_IfSummaryStarsExistsReturnsConvertedModel()
        {
            // Arrange
            var summaries = new Fixture().CreateMany<EmployerFeedbackResultSummary>(10).ToList();
            mockRepository
                .Setup(s => s.GetAllStarsSummary(It.IsAny<string>()))
                .ReturnsAsync(summaries);

            // Act
            var response = await handler.Handle(new ProviderSummaryStarsQuery(), new CancellationToken());

            // Assert
            response.Should().BeAssignableTo<IEnumerable<EmployerFeedbackStarsSummaryDto>>();
            response.Should().NotBeEmpty();

            response.Should().BeEquivalentTo(summaries.Select(s => new EmployerFeedbackStarsSummaryDto
            {
                Ukprn = s.Ukprn,
                ReviewCount = s.ReviewCount,
                Stars = s.Stars,
                TimePeriod = s.TimePeriod
            }));
        }
    }
}
