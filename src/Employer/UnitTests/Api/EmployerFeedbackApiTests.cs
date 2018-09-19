namespace UnitTests.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using ESFA.DAS.EmployerProvideFeedback.Api.Configuration;
    using ESFA.DAS.EmployerProvideFeedback.Api.Controllers;
    using ESFA.DAS.EmployerProvideFeedback.Api.Dto;
    using ESFA.DAS.EmployerProvideFeedback.Api.Repository;
    using ESFA.DAS.EmployerProvideFeedback.Configuration;
    using ESFA.DAS.FeedbackDataAccess.Repositories;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Moq;

    using Newtonsoft.Json;

    using Xunit;

    using IEmployerFeedbackRepository = ESFA.DAS.EmployerProvideFeedback.Api.Repository.IEmployerFeedbackRepository;

    public class FeedbackControllerTests : IDisposable
    {
        private readonly FeedbackController controller;

        private readonly Mock<ILogger<FeedbackController>> mockLogger;

        private readonly Mock<IEmployerFeedbackRepository> mockRepo;

        private readonly IOptions<AzureOptions> options;

        private readonly List<EmployerFeedback> testData = new List<EmployerFeedback>();

        private readonly EmployerFeedbackTestHelper testHelper;

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

            this.mockRepo = new Mock<IEmployerFeedbackRepository>();

            this.mockLogger = new Mock<ILogger<FeedbackController>>();

            this.testHelper = new EmployerFeedbackTestHelper();

            for (var i = 0; i < 50; i++)
            {
                this.testData.Add(this.testHelper.GenerateRandomFeedback(Guid.NewGuid().ToString()));
            }

            this.testData.Add(this.testHelper.GenerateRandomFeedback("ddcf9d13-bf05-4e9c-bd5c-20c4133cc739"));
            this.testData.Add(this.testHelper.GenerateRandomFeedback("84C4CFDD-1DEA-4A1A-BEC8-C1A6C763004C"));

            this.controller = new FeedbackController(this.mockRepo.Object, this.mockLogger.Object);
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
                this.mockRepo.Setup(m => m.GetAllItemsAsync(It.IsAny<FeedOptions>()))
                    .ReturnsAsync(this.testData);
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

                var model = actionOkResult.Value as List<EmployerFeedback>;
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
                var queries = new List<Expression<Func<EmployerFeedback, bool>>>();
                foreach (var t in this.testData)
                {
                    Expression<Func<EmployerFeedback, bool>> expression = i => i.Id == t.Id;
                    queries.Add(expression);
                }

                queries.Add(i => i.Id == "00000000-0000-0000-0000-000000000000");

                this.mockRepo
                    .Setup(
                        m => m.GetItemAsync(
                            It.IsAny<Expression<Func<EmployerFeedback, bool>>>(),
                            It.IsAny<FeedOptions>())).ReturnsAsync(
                        (Expression<Func<EmployerFeedback, bool>> x, FeedOptions y) =>
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

                var actual = actionOkResult.Value as EmployerFeedback;
                Assert.NotNull(actual);

                Assert.Equal(expected.Id, actual.Id);
                Assert.Equal(expected.Ukprn, actual.Ukprn);
                Assert.Equal(expected.AccountId, actual.AccountId);
                Assert.Equal(expected.UserRef, actual.UserRef);
                Assert.Equal(expected.DateTimeCompleted, actual.DateTimeCompleted);
                Assert.Equal(expected.ProviderAttributes.Count, actual.ProviderAttributes.Count);
                Assert.Equal(expected.ProviderRating, actual.ProviderRating);
            }
        }
    }
}