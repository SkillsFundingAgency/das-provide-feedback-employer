namespace UnitTests.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using AutoMapper;

    using ESFA.DAS.EmployerProvideFeedback.Api.Configuration;
    using ESFA.DAS.EmployerProvideFeedback.Api.Controllers;
    using ESFA.DAS.EmployerProvideFeedback.Api.Models;
    using ESFA.DAS.EmployerProvideFeedback.Api.Repository;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Moq;

    using Newtonsoft.Json;

    using Xunit;

    using EmployerFeedbackDto = ESFA.DAS.EmployerProvideFeedback.Api.Dto.EmployerFeedback;
    using ProviderAttributeDto = ESFA.DAS.EmployerProvideFeedback.Api.Dto.ProviderAttribute;

    public class FeedbackControllerTests : IDisposable
    {
        private readonly FeedbackController controller;

        private readonly Mock<ILogger<FeedbackController>> mockLogger;

        private readonly Mock<IEmployerFeedbackRepository> mockRepo;

        private readonly IOptions<AzureOptions> options;

        private readonly List<EmployerFeedbackDto> testData = new List<EmployerFeedbackDto>();

        private readonly EmployerFeedbackTestHelper testHelper;

        private IMapper mapper;

        public FeedbackControllerTests()
        {
            this.options = Options.Create(
                new AzureOptions
                    {
                        CosmosEndpoint = string.Empty,
                        CosmosKey = string.Empty,
                        DatabaseName = string.Empty,
                        EmployerFeedbackCollection = string.Empty
                    });

            this.mapper = new Mapper(new MapperConfiguration(ConfigureMaps));

            this.mockRepo = new Mock<IEmployerFeedbackRepository>();

            this.mockLogger = new Mock<ILogger<FeedbackController>>();

            this.testHelper = new EmployerFeedbackTestHelper();

            for (var i = 0; i < 50; i++)
            {
                this.testData.Add(this.testHelper.GenerateRandomFeedback(Guid.NewGuid().ToString()));
            }

            this.testData.Add(this.testHelper.GenerateRandomFeedback("ddcf9d13-bf05-4e9c-bd5c-20c4133cc739"));
            this.testData.Add(this.testHelper.GenerateRandomFeedback("84C4CFDD-1DEA-4A1A-BEC8-C1A6C763004C"));

            this.controller = new FeedbackController(this.mockRepo.Object, this.mockLogger.Object, this.mapper);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="FeedbackControllerTests"/> class. 
        /// </summary>
        ~FeedbackControllerTests()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static void ConfigureMaps(IMapperConfigurationExpression config)
        {
            config.CreateMap<ProviderAttributeDto, ProviderAttribute>()
                .ForMember(destination => destination.Name, opt => opt.MapFrom(src => src.Name)).ForMember(
                    destination => destination.Value,
                    opt => opt.MapFrom(src => src.Value));

            config.CreateMap<EmployerFeedbackDto, PublicEmployerFeedback>()
                .ForMember(
                    destination => destination.DateTimeCompleted,
                    opt => opt.MapFrom(src => src.DateTimeCompleted))
                .ForMember(
                    destination => destination.ProviderAttributes,
                    opt => opt.MapFrom(src => src.ProviderAttributes))
                .ForMember(destination => destination.ProviderRating, opt => opt.MapFrom(src => src.ProviderRating))
                .ForMember(destination => destination.Ukprn, opt => opt.MapFrom(src => src.Ukprn));

            config.CreateMap<EmployerFeedbackDto, EmployerFeedback>()
                .ForMember(destination => destination.AccountId, opt => opt.MapFrom(src => src.AccountId))
                .ForMember(destination => destination.UserRef, opt => opt.MapFrom(src => src.UserRef))
                .ForMember(
                    destination => destination.DateTimeCompleted,
                    opt => opt.MapFrom(src => src.DateTimeCompleted))
                .ForMember(
                    destination => destination.ProviderAttributes,
                    opt => opt.MapFrom(src => src.ProviderAttributes))
                .ForMember(destination => destination.ProviderRating, opt => opt.MapFrom(src => src.ProviderRating))
                .ForMember(destination => destination.Ukprn, opt => opt.MapFrom(src => src.Ukprn));
        }

        private void Dispose(bool disposing)
        {
            this.ReleaseUnmanagedResources();
            if (disposing)
            {
                this.controller?.Dispose();
            }
        }

        private void ReleaseUnmanagedResources()
        {
            this.testData.Clear();
            this.controller.Dispose();
        }

        public class GetAll : FeedbackControllerTests
        {
            public GetAll()
            {
                this.mockRepo.Setup(m => m.GetAllItemsAsync(It.IsAny<FeedOptions>())).ReturnsAsync(this.testData);
            }

            [Fact]
            public async Task Should_Return_All_Items()
            {
                // arrange
                var expected = this.testData.Count;

                // act
                var actionResult = await this.controller.GetAll();

                // assert
                var actionOkResult = actionResult as OkObjectResult;
                Assert.NotNull(actionOkResult);

                var model = actionOkResult.Value as List<PublicEmployerFeedback>;
                Assert.NotNull(model);

                var actual = model.Count;

                Assert.Equal(expected, actual);
            }
        }

        public class GetById : FeedbackControllerTests
        {
            public GetById()
                : base()
            {
                var queries = new List<Expression<Func<EmployerFeedbackDto, bool>>>();
                foreach (var t in this.testData)
                {
                    Expression<Func<EmployerFeedbackDto, bool>> expression = i => i.Id == t.Id;
                    queries.Add(expression);
                }

                queries.Add(i => i.Id == "00000000-0000-0000-0000-000000000000");

                this.mockRepo
                    .Setup(
                        m => m.GetItemAsync(
                            It.IsAny<Expression<Func<EmployerFeedbackDto, bool>>>(),
                            It.IsAny<FeedOptions>())).ReturnsAsync(
                        (Expression<Func<EmployerFeedbackDto, bool>> x, FeedOptions y) =>
                            this.testData.SingleOrDefault(x.Compile()));
            }

            [Fact]
            public async Task Should_Return_NotFound_If_Not_Present()
            {
                // act
                var actionResult = await this.controller.GetById("00000000-0000-0000-0000-000000000000");
                Console.WriteLine(JsonConvert.SerializeObject(actionResult));

                // assert
                var actionNotFoundResult = actionResult as NotFoundResult;
                Assert.NotNull(actionNotFoundResult);
            }

            [Theory]
            [InlineData("ddcf9d13-bf05-4e9c-bd5c-20c4133cc739")]
            [InlineData("84C4CFDD-1DEA-4A1A-BEC8-C1A6C763004C")]
            public async Task Should_Return_The_Correct_Item(string id)
            {
                // arrange
                var expected = this.testData.Single(t => t.Id == id.ToLower());

                // act
                var actionResult = await this.controller.GetById(id.ToLower());
                Console.WriteLine(JsonConvert.SerializeObject(actionResult));

                // assert
                Assert.NotNull(actionResult);
                var actionOkResult = actionResult as OkObjectResult;
                Assert.NotNull(actionOkResult);

                var actual = actionOkResult.Value as PublicEmployerFeedback;
                Assert.NotNull(actual);

                Assert.Equal(expected.Ukprn, actual.Ukprn);
                Assert.Equal(expected.DateTimeCompleted, actual.DateTimeCompleted);
                Assert.Equal(expected.ProviderAttributes.Count, actual.ProviderAttributes.Count);
                Assert.Equal(expected.ProviderRating, actual.ProviderRating);
            }
        }
    }
}