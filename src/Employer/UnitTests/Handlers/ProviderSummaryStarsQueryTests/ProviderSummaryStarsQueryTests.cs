
using AutoFixture;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;
using ESFA.DAS.EmployerProvideFeedback.Api.Queries.ProviderSummaryStarsQuery;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;


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


        [Fact]
        public async Task WhenQueryingProviderSummaryStars_IfNullReturnsNull()
        {
            // arrange
            mockRepository.Setup(s => s.GetAllStarsSummary()).ReturnsAsync((IEnumerable<ProviderStarsSummary>)null);

            // act
            IEnumerable<EmployerFeedbackStarsSummaryDto> response = await handler.Handle(new ProviderSummaryStarsQuery(), new CancellationToken());

            // assert
            Assert.Null(response);
        }

        [Fact]
        public async Task WhenQueryingProviderSummaryStars_IfNoSummaryStarsReturnsEmptyCollection()
        {
            // arrange
            mockRepository.Setup(s => s.GetAllStarsSummary()).ReturnsAsync(new List<ProviderStarsSummary>());

            // act
            IEnumerable<EmployerFeedbackStarsSummaryDto> response = await handler.Handle(new ProviderSummaryStarsQuery(), new CancellationToken());

            // assert
            Assert.IsAssignableFrom<IEnumerable<EmployerFeedbackStarsSummaryDto>>(response);
            Assert.Empty(response);
        }

        [Fact]
        public async Task WhenQueryingProviderSummaryStars_IfSummaryStarsExistsReturnsConvertedModel()
        {
            // arrange
            IEnumerable<EmployerFeedbackResultSummary> summaries = new Fixture().CreateMany<EmployerFeedbackResultSummary>(10);
            mockRepository.Setup(s => s.GetAllStarsSummary()).ReturnsAsync(summaries);

            // act
            IEnumerable<EmployerFeedbackStarsSummaryDto> response = await handler.Handle(new ProviderSummaryStarsQuery(), new CancellationToken());

            // assert
            Assert.IsAssignableFrom<IEnumerable<EmployerFeedbackStarsSummaryDto>>(response);
            Assert.NotEmpty(response);

            response.Should().BeEquivalentTo(summaries.Select(s => new EmployerFeedbackStarsSummaryDto()
            {
                Ukprn = s.Ukprn,
                ReviewCount = s.ReviewCount,
                Stars = s.Stars
            }));
        }
    }
}
