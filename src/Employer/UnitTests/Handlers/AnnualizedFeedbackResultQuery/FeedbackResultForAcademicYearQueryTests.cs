
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
using ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackResultForAcademicYearQuery;

namespace UnitTests.Api
{
    public class FeedbackResultForAcademicYearQueryTests
    {
        private readonly Mock<ILogger<FeedbackResultForAcademicYearQueryHandler>> mockLogger;
        private readonly Mock<IEmployerFeedbackRepository> mockRepository;
        private readonly FeedbackResultForAcademicYearQueryHandler handler;

        public FeedbackResultForAcademicYearQueryTests()
        {
            mockLogger = new Mock<ILogger<FeedbackResultForAcademicYearQueryHandler>>();
            mockRepository = new Mock<IEmployerFeedbackRepository>();
            handler = new FeedbackResultForAcademicYearQueryHandler(mockRepository.Object, mockLogger.Object);
        }

        [Fact]
        public async Task WhenQueryingFeedbackResultForAcademicYear_IfNullReturnsEmptyCollection()
        {
            // arrange
            mockRepository.Setup(s => s.GetFeedbackResultSummaryForAcademicYear(123, "AY2324")).ReturnsAsync((IEnumerable<EmployerFeedbackResultSummary>)null);

            // act
            EmployerFeedbackForAcademicYearResultDto response = await handler.Handle(new FeedbackResultForAcademicYearQuery() { Ukprn = 123 }, new CancellationToken());

            // assert
            Assert.Equal(123, response.Ukprn);
            Assert.Equal(0, response.ReviewCount);
            Assert.Equal(0, response.Stars);
            Assert.IsAssignableFrom<IEnumerable<ProviderAttributeForAcademicYearSummaryItemDto>>(response.ProviderAttribute);
            Assert.Empty(response.ProviderAttribute);
        }

        [Fact]
        public async Task WhenQueryingFeedbackResultForAcademicYear_IfNoFeedbackReturnsEmptyCollection()
        {
            // arrange
            mockRepository.Setup(s => s.GetFeedbackResultSummaryForAcademicYear(456, "AY2324")).ReturnsAsync(new List<EmployerFeedbackResultSummary>());

            // act
            EmployerFeedbackForAcademicYearResultDto response = await handler.Handle(new FeedbackResultForAcademicYearQuery() { Ukprn = 456 }, new CancellationToken());

            // assert
            Assert.Equal(456, response.Ukprn);
            Assert.Equal(0, response.ReviewCount);
            Assert.Equal(0, response.Stars);
            Assert.IsAssignableFrom<IEnumerable<ProviderAttributeForAcademicYearSummaryItemDto>>(response.ProviderAttribute);
            Assert.Empty(response.ProviderAttribute);
        }

        [Fact]
        public async Task WhenQueryingFeedbackResultForAcademicYear_IfFeedbackExistsReturnsConvertedModel()
        {
            // arrange
            IEnumerable<EmployerFeedbackResultSummary> summaries = new Fixture().CreateMany<EmployerFeedbackResultSummary>(1);
            summaries.First().Ukprn = 789;
            mockRepository.Setup(s => s.GetFeedbackResultSummaryForAcademicYear(789, "AY2324")).ReturnsAsync(summaries);

            // act
            EmployerFeedbackForAcademicYearResultDto response = await handler.Handle(new FeedbackResultForAcademicYearQuery() { Ukprn = 789, AcademicYear = "AY2324" }, new CancellationToken());

            // assert
            EmployerFeedbackResultSummary summary = summaries.First();
            Assert.Equal(summary.Ukprn, response.Ukprn);
            Assert.Equal(summary.ReviewCount, response.ReviewCount);
            Assert.Equal(summary.Stars, response.Stars);
            Assert.IsAssignableFrom<IEnumerable<ProviderAttributeForAcademicYearSummaryItemDto>>(response.ProviderAttribute);
            Assert.NotEmpty(response.ProviderAttribute);
        }
    }
}
