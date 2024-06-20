
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
using ESFA.DAS.EmployerProvideFeedback.Api.Queries.AnnualizedFeedbackResultQuery;


namespace UnitTests.Api
{

    public class AnnualizedFeedbackResultQueryTests
    {
        private readonly Mock<ILogger<AnnualizedFeedbackResultQueryHandler>> mockLogger;
        private readonly Mock<IEmployerFeedbackRepository> mockRepository;
        private readonly AnnualizedFeedbackResultQueryHandler handler;

        public AnnualizedFeedbackResultQueryTests()
        {
            mockLogger = new Mock<ILogger<AnnualizedFeedbackResultQueryHandler>>();
            mockRepository = new Mock<IEmployerFeedbackRepository>();
            handler = new AnnualizedFeedbackResultQueryHandler(mockRepository.Object, mockLogger.Object);
        }


        [Fact]
        public async Task WhenQueryingAnnualizedFeedbackResult_IfNullReturnsEmptyCollection()
        {
            // arrange
            mockRepository.Setup(s => s.GetAnnualizedFeedbackResultSummary(123,"AY2425")).ReturnsAsync((IEnumerable<EmployerFeedbackResultSummary>) null);

            // act
            EmployerAnnualizedFeedbackResultDto response = await handler.Handle(new AnnualizedFeedbackResultQuery() { Ukprn = 123,AcademicYear = "AY2425" }, new CancellationToken());

            // assert
            Assert.Equal(123, response.Ukprn);
            Assert.Equal(0, response.ReviewCount);
            Assert.Equal(0, response.Stars);
            Assert.IsAssignableFrom<IEnumerable<ProviderAttributeAnnualizedSummaryItemDto>>(response.ProviderAttribute);
            Assert.Empty(response.ProviderAttribute);
        }

        [Fact]
        public async Task WhenQueryingAnnualizedFeedbackResult_IfNoFeedbackReturnsEmptyCollection()
        {
            // arrange
            mockRepository.Setup(s => s.GetAnnualizedFeedbackResultSummary(456, "AY2425")).ReturnsAsync(new List<EmployerFeedbackResultSummary>());

            // act
            EmployerAnnualizedFeedbackResultDto response = await handler.Handle(new AnnualizedFeedbackResultQuery() { Ukprn = 456,AcademicYear = "AY2425" }, new CancellationToken());

            // assert
            Assert.Equal(456, response.Ukprn);
            Assert.Equal(0, response.ReviewCount);
            Assert.Equal(0, response.Stars);
            Assert.IsAssignableFrom<IEnumerable<ProviderAttributeAnnualizedSummaryItemDto>>(response.ProviderAttribute);
            Assert.Empty(response.ProviderAttribute);
        }

        [Fact]
        public async Task WhenQueryingAnnualizedFeedbackResult_IfFeedbackExistsReturnsConvertedModel()
        {
            // arrange
            IEnumerable<EmployerFeedbackResultSummary> summaries = new Fixture().CreateMany<EmployerFeedbackResultSummary>(1);
            summaries.First().Ukprn = 789;
            mockRepository.Setup(s => s.GetAnnualizedFeedbackResultSummary(789, "AY2425")).ReturnsAsync(summaries);

            // act
            EmployerAnnualizedFeedbackResultDto response = await handler.Handle(new AnnualizedFeedbackResultQuery() { Ukprn = 789,AcademicYear = "AY2425" }, new CancellationToken());

            // assert
            EmployerFeedbackResultSummary summary = summaries.First();
            Assert.Equal(summary.Ukprn, response.Ukprn);
            Assert.Equal(summary.ReviewCount, response.ReviewCount);
            Assert.Equal(summary.Stars, response.Stars);
            Assert.IsAssignableFrom<IEnumerable<ProviderAttributeAnnualizedSummaryItemDto>>(response.ProviderAttribute);
            Assert.NotEmpty(response.ProviderAttribute);
        }
    }
}
