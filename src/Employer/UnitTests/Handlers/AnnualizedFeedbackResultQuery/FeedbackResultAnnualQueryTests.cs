using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackResultAnnualQuery;

namespace UnitTests.Api
{
    public class FeedbackResultAnnualQueryTests
    {
        private readonly Mock<ILogger<FeedbackResultAnnualQueryHandler>> mockLogger;
        private readonly Mock<IEmployerFeedbackRepository> mockRepository;
        private readonly FeedbackResultAnnualQueryHandler handler;

        public FeedbackResultAnnualQueryTests()
        {
            mockLogger = new Mock<ILogger<FeedbackResultAnnualQueryHandler>>();
            mockRepository = new Mock<IEmployerFeedbackRepository>();
            handler = new FeedbackResultAnnualQueryHandler(mockRepository.Object, mockLogger.Object);
        }

        [Fact]
        public async Task WhenQueryingFeedbackResultAnnual_IfNullReturnsEmptyCollection()
        {
            // arrange
            mockRepository.Setup(s => s.GetFeedbackResultSummaryAnnual(123)).ReturnsAsync((IEnumerable<EmployerFeedbackResultSummary>)null);

            // act
            var response = await handler.Handle(new FeedbackResultAnnualQuery() { Ukprn = 123 }, new CancellationToken());

            // assert
            Assert.NotNull(response);
            Assert.Empty(response.AnnualEmployerFeedbackDetails);
        }

        [Fact]
        public async Task WhenQueryingFeedbackResultAnnual_IfNoFeedbackReturnsEmptyCollection()
        {
            // arrange
            mockRepository.Setup(s => s.GetFeedbackResultSummaryAnnual(456)).ReturnsAsync(new List<EmployerFeedbackResultSummary>());

            // act
            var response = await handler.Handle(new FeedbackResultAnnualQuery() { Ukprn = 456 }, new CancellationToken());

            // assert
            Assert.NotNull(response);
            Assert.Empty(response.AnnualEmployerFeedbackDetails);
        }

        [Fact]
        public async Task WhenQueryingFeedbackResultAnnual_IfFeedbackExistsReturnsConvertedModel()
        {
            // arrange
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

            mockRepository.Setup(s => s.GetFeedbackResultSummaryAnnual(789)).ReturnsAsync(summaries);

            // act
            var response = await handler.Handle(new FeedbackResultAnnualQuery() { Ukprn = 789 }, new CancellationToken());

            // assert
            Assert.NotNull(response);
            Assert.NotEmpty(response.AnnualEmployerFeedbackDetails);

            var details = response.AnnualEmployerFeedbackDetails.First();
            Assert.Equal(789, details.Ukprn);
            Assert.Equal("All", details.TimePeriod);
            Assert.Equal(4, details.Stars);
            Assert.Equal(10, details.ReviewCount);
            Assert.Equal(2, details.ProviderAttribute.Count());
        }
    }
}
