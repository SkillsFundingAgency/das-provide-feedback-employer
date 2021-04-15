using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using ESFA.DAS.ProvideFeedback.Domain.Entities.ApiTypes;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.Configuration;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace UnitTests.ApplicationServices
{
    public class WhenGettingAllProviders
    {
        [Fact]
        public async Task Then_The_Url_Is_Called_And_Data_Returned()
        {
            //Arrange
            var fixture = new Fixture();
            var identifier = fixture.Create<string>();
            var providersUrl = fixture.Create<string>();
            var authToken = fixture.Create<string>();
            var importProviders = fixture.CreateMany<ProviderRegistration>();
            providersUrl = $"https://test.local/{providersUrl}/";

            var azureClientCredentialHelper = new Mock<IAzureClientCredentialHelper>();
            azureClientCredentialHelper.Setup(x => x.GetAccessTokenAsync(identifier)).ReturnsAsync(authToken);

            var configuration = new Mock<IOptions<RoatpApiConfiguration>>();
            configuration.Setup(x => x.Value).Returns(new RoatpApiConfiguration
            {
                Identifier =  identifier,
                Url = providersUrl
            });
            var response = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(importProviders)),
                StatusCode = HttpStatusCode.Accepted
            };
            var httpMessageHandler = SetupMessageHandlerMock(response, new Uri(providersUrl + "v1/fat-data-export"), HttpMethod.Get, authToken);
            var client = new HttpClient(httpMessageHandler.Object);
            var apprenticeshipService = new RoatpService(client,azureClientCredentialHelper.Object,Mock.Of<IHostingEnvironment>(), configuration.Object);
            
            //Act
            var providers = await apprenticeshipService.GetAll();
            
            //Assert
            providers.Should().BeEquivalentTo(importProviders);
        }
        
        private static Mock<HttpMessageHandler> SetupMessageHandlerMock(HttpResponseMessage response, Uri uri, HttpMethod httpMethod, string authToken)
        {
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(c =>
                        c.Method.Equals(httpMethod)
                        && c.RequestUri.Equals(uri)
                        && c.Headers.Authorization.Scheme.Equals("Bearer")
                        && c.Headers.Authorization.Parameter.Equals(authToken)
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync((HttpRequestMessage request, CancellationToken token) => response);
            return httpMessageHandler;
        }
    }
}