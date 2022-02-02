namespace UnitTests.Api
{
    using AutoFixture;
    using AutoFixture.Xunit2;
    using ESFA.DAS.EmployerProvideFeedback.Api.Controllers;
    using ESFA.DAS.EmployerProvideFeedback.Api.Models;
    using ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackQuery;
    using ESFA.DAS.ProvideFeedback.Data.Repositories;
    using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
    using FluentAssertions;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class FeedbackQueryTests
    {
        private readonly FeedbackQueryHandler handler;

        private readonly Mock<ILogger<FeedbackQueryHandler>> mockLogger;

        private readonly Mock<IEmployerFeedbackRepository> mockRepository;


        public FeedbackQueryTests()
        {
            mockLogger = new Mock<ILogger<FeedbackQueryHandler>>();
            mockRepository = new Mock<IEmployerFeedbackRepository>();
            handler = new FeedbackQueryHandler(mockRepository.Object, mockLogger.Object);
        }


        [Fact]
        public async Task WhenQueryingFeedback_IfNullReturnsEmptyCollection()
        {
            // arrange
            mockRepository.Setup(s => s.GetEmployerFeedback()).ReturnsAsync((IEnumerable<EmployerFeedbackViewModel>) null);

            // act
            var response = await handler.Handle(new FeedbackQuery(), new CancellationToken());

            // assert
            response.Should().BeEquivalentTo(Enumerable.Empty<EmployerFeedbackDto>());
        }

        [Fact]
        public async Task WhenQueryingFeedback_IfNoFeedbackReturnsEmptyCollection()
        {
            // arrange
            mockRepository.Setup(s => s.GetEmployerFeedback()).ReturnsAsync(new List<EmployerFeedbackViewModel>());

            // act
            var response = await handler.Handle(new FeedbackQuery(), new CancellationToken());

            // assert
            response.Should().BeEquivalentTo(Enumerable.Empty<EmployerFeedbackDto>());
        }

        [Fact]
        public async Task WhenQueryingFeedback_IfFeedbackExistsReturnsConvertedModel()
        {
            // arrange
            var fixture = new Fixture();
            var feedback = fixture.CreateMany<EmployerFeedbackViewModel>(150);
            mockRepository.Setup(s => s.GetEmployerFeedback()).ReturnsAsync(feedback);

            // act
            var response = await handler.Handle(new FeedbackQuery(), new CancellationToken());

            // assert
            response.Should().BeEquivalentTo(feedback.Select(s => new EmployerFeedbackDto
            {
                Ukprn = s.Ukprn,
                ProviderRating = s.ProviderRating,
                DateTimeCompleted = s.DateTimeCompleted,
                ProviderAttributes = new List<ProviderAttributeDto> { new ProviderAttributeDto { Name = s.AttributeName, Value = s.AttributeValue } }
            }));
        }

        [Theory, AutoData]
        public async Task WhenQueryingFeedback_IfFeedbackExistsReturnsGroupedFeedback(Guid Id, long ukprn, string providerRating, DateTime dateTimeCompleted)
        {
            // arrange
            var fixture = new Fixture();
            var feedback = fixture.CreateMany<EmployerFeedbackViewModel>(10);
            foreach(var f in feedback)
            {
                f.Id = Id;
                f.Ukprn = ukprn;
                f.ProviderRating = providerRating;
                f.DateTimeCompleted = dateTimeCompleted;
            }
            mockRepository.Setup(s => s.GetEmployerFeedback()).ReturnsAsync(feedback);

            // act
            var response = await handler.Handle(new FeedbackQuery(), new CancellationToken());

            // assert
            var attributes = feedback.Select(p => new ProviderAttributeDto { Name = p.AttributeName, Value = p.AttributeValue }).ToList();
            response.Should().BeEquivalentTo(new EmployerFeedbackDto
            {
                Ukprn = ukprn,
                ProviderRating = providerRating,
                DateTimeCompleted = dateTimeCompleted,
                ProviderAttributes = attributes
            });
        }
    }
}