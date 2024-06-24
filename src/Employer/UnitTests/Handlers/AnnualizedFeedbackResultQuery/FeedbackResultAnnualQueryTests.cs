using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;
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
            EmployerFeedbackAnnualResultDto response = await handler.Handle(new FeedbackResultAnnualQuery() { Ukprn = 123 }, new CancellationToken());

            // assert
            Assert.Equal(123, response.Ukprn);
            Assert.Equal(0, response.ReviewCount);
            Assert.Equal(0, response.Stars);
            Assert.IsAssignableFrom<IEnumerable<ProviderAttributeAnnualSummaryItemDto>>(response.ProviderAttribute);
            Assert.Empty(response.ProviderAttribute);
        }

        [Fact]
        public async Task WhenQueryingFeedbackResultAnnual_IfNoFeedbackReturnsEmptyCollection()
        {
            // arrange
            mockRepository.Setup(s => s.GetFeedbackResultSummaryAnnual(456)).ReturnsAsync(new List<EmployerFeedbackResultSummary>());

            // act
            EmployerFeedbackAnnualResultDto response = await handler.Handle(new FeedbackResultAnnualQuery() { Ukprn = 456 }, new CancellationToken());

            // assert
            Assert.Equal(456, response.Ukprn);
            Assert.Equal(0, response.ReviewCount);
            Assert.Equal(0, response.Stars);
            Assert.IsAssignableFrom<IEnumerable<ProviderAttributeAnnualSummaryItemDto>>(response.ProviderAttribute);
            Assert.Empty(response.ProviderAttribute);
        }

        [Fact]
        public async Task WhenQueryingFeedbackResultAnnual_IfFeedbackExistsReturnsConvertedModel()
        {
            // arrange
            IEnumerable<EmployerFeedbackResultSummary> summaries = new Fixture().CreateMany<EmployerFeedbackResultSummary>(1);
            summaries.First().Ukprn = 789;
            mockRepository.Setup(s => s.GetFeedbackResultSummaryAnnual(789)).ReturnsAsync(summaries);

            // act
            EmployerFeedbackAnnualResultDto response = await handler.Handle(new FeedbackResultAnnualQuery() { Ukprn = 789 }, new CancellationToken());

            // assert
            EmployerFeedbackResultSummary summary = summaries.First();
            Assert.Equal(summary.Ukprn, response.Ukprn);
            Assert.Equal(summary.ReviewCount, response.ReviewCount);
            Assert.Equal(summary.Stars, response.Stars);
            Assert.IsAssignableFrom<IEnumerable<ProviderAttributeAnnualSummaryItemDto>>(response.ProviderAttribute);
            Assert.NotEmpty(response.ProviderAttribute);
        }
    }
}
