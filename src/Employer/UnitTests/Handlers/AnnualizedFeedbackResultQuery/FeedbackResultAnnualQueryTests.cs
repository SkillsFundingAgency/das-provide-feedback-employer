using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackResultAnnualQuery;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace UnitTests.Api
{
    public class FeedbackResultAnnualQueryTests
    {
        private Mock<ILogger<FeedbackResultAnnualQueryHandler>> mockLogger;
        private Mock<IEmployerFeedbackRepository> mockRepository;
        private FeedbackResultAnnualQueryHandler handler;

        [SetUp]
        public void Setup()
        {
            mockLogger = new Mock<ILogger<FeedbackResultAnnualQueryHandler>>();
            mockRepository = new Mock<IEmployerFeedbackRepository>();
            handler = new FeedbackResultAnnualQueryHandler(mockRepository.Object, mockLogger.Object);
        }

        [Test]
        public async Task WhenQueryingFeedbackResultAnnual_IfNullReturnsEmptyCollection()
        {
            // Arrange
            mockRepository.Setup(s => s.GetFeedbackResultSummaryAnnual(123))
                .ReturnsAsync((IEnumerable<EmployerFeedbackResultSummary>)null);

            // Act
            var response = await handler.Handle(new FeedbackResultAnnualQuery { Ukprn = 123 }, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.AnnualEmployerFeedbackDetails.Should().BeEmpty();
        }

        [Test]
        public async Task WhenQueryingFeedbackResultAnnual_IfNoFeedbackReturnsEmptyCollection()
        {
            // Arrange
            mockRepository.Setup(s => s.GetFeedbackResultSummaryAnnual(456))
                .ReturnsAsync(new List<EmployerFeedbackResultSummary>());

            // Act
            var response = await handler.Handle(new FeedbackResultAnnualQuery { Ukprn = 456 }, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.AnnualEmployerFeedbackDetails.Should().BeEmpty();
        }

        [Test]
        public async Task WhenQueryingFeedbackResultAnnual_IfFeedbackExistsReturnsConvertedModel()
        {
            // Arrange
            var summaries = new List<EmployerFeedbackResultSummary>
            {
                new EmployerFeedbackResultSummary
                {
                    Ukprn = 789,
                    TimePeriod = "All",
                    Stars = 4,
                    ReviewCount = 10,
                    AttributeName = "Providing the right training at the right time",
                    Strength = 2,
                    Weakness = 4
                },
                new EmployerFeedbackResultSummary
                {
                    Ukprn = 789,
                    TimePeriod = "All",
                    Stars = 4,
                    ReviewCount = 10,
                    AttributeName = "Communication with employers",
                    Strength = 2,
                    Weakness = 4
                }
            };

            mockRepository.Setup(s => s.GetFeedbackResultSummaryAnnual(789))
                .ReturnsAsync(summaries);

            // Act
            var response = await handler.Handle(new FeedbackResultAnnualQuery { Ukprn = 789 }, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.AnnualEmployerFeedbackDetails.Should().NotBeEmpty();

            var details = response.AnnualEmployerFeedbackDetails.First();
            details.Ukprn.Should().Be(789);
            details.TimePeriod.Should().Be("All");
            details.Stars.Should().Be(4);
            details.ReviewCount.Should().Be(10);
            details.ProviderAttribute.Should().HaveCount(2);
        }
    }
}
