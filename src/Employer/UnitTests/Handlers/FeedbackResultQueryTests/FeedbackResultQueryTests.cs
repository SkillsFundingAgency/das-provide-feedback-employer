
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;
using ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackResultQuery;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;


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


        [Fact]
        public async Task WhenQueryingFeedbackResult_IfNullReturnsEmptyCollection()
        {
            // arrange
            mockRepository.Setup(s => s.GetFeedbackResultSummary(123)).ReturnsAsync((IEnumerable<EmployerFeedbackResultSummary>) null);

            // act
            EmployerFeedbackResultDto response = await handler.Handle(new FeedbackResultQuery() { Ukprn = 123 }, new CancellationToken());

            // assert
            Assert.Equal(123, response.Ukprn);
            Assert.Equal(0, response.ReviewCount);
            Assert.Equal(0, response.Stars);
            Assert.IsAssignableFrom<IEnumerable<ProviderAttributeSummaryItemDto>>(response.ProviderAttribute);
            Assert.Empty(response.ProviderAttribute);
        }

        [Fact]
        public async Task WhenQueryingFeedbackResult_IfNoFeedbackReturnsEmptyCollection()
        {
            // arrange
            mockRepository.Setup(s => s.GetFeedbackResultSummary(456)).ReturnsAsync(new List<EmployerFeedbackResultSummary>());

            // act
            EmployerFeedbackResultDto response = await handler.Handle(new FeedbackResultQuery() { Ukprn = 456 }, new CancellationToken());

            // assert
            Assert.Equal(456, response.Ukprn);
            Assert.Equal(0, response.ReviewCount);
            Assert.Equal(0, response.Stars);
            Assert.IsAssignableFrom<IEnumerable<ProviderAttributeSummaryItemDto>>(response.ProviderAttribute);
            Assert.Empty(response.ProviderAttribute);
        }

        [Fact]
        public async Task WhenQueryingFeedbackResult_IfFeedbackExistsReturnsConvertedModel()
        {
            // arrange
            IEnumerable<EmployerFeedbackResultSummary> summaries = new Fixture().CreateMany<EmployerFeedbackResultSummary>(1);
            summaries.First().Ukprn = 789;
            mockRepository.Setup(s => s.GetFeedbackResultSummary(789)).ReturnsAsync(summaries);

            // act
            EmployerFeedbackResultDto response = await handler.Handle(new FeedbackResultQuery() { Ukprn = 789 }, new CancellationToken());

            // assert
            EmployerFeedbackResultSummary summary = summaries.First();
            Assert.Equal(summary.Ukprn, response.Ukprn);
            Assert.Equal(summary.ReviewCount, response.ReviewCount);
            Assert.Equal(summary.Stars, response.Stars);
            Assert.IsAssignableFrom<IEnumerable<ProviderAttributeSummaryItemDto>>(response.ProviderAttribute);
            Assert.NotEmpty(response.ProviderAttribute);
        }
    }
}
