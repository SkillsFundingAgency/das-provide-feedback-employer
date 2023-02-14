using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.Configuration;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.OuterApi;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;

namespace UnitTests.OuterApi
{
    public class WhenCallingGet
    { 
        [Test, AutoData]
        public async Task Then_The_Endpoint_Is_Called_With_Authentication_Header_And_Data_Returned(
            List<string> testObject, 
            OuterApiConfiguration config)
        {
            //Arrange
            config.BaseUrl = $"https://{config.BaseUrl}";
            var getTestRequest = new GetTestRequest();
            
            var response = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(testObject)),
                StatusCode = HttpStatusCode.Accepted
            };
            var httpMessageHandler = SetupMessageHandlerMock(response, new Uri(config.BaseUrl + getTestRequest.GetUrl), config.Key, HttpMethod.Get);
            var client = new HttpClient(httpMessageHandler.Object);
            var apiClient = new OuterApiClient(client, config);

            //Act
            var actual = await apiClient.Get<List<string>>(getTestRequest);
            
            //Assert
            actual.Body.Should().BeEquivalentTo(testObject);
        }
        
        [Test, AutoData]
        public async Task Then_If_It_Is_Not_Successful_An_Error_Is_Returned(
            OuterApiConfiguration config)
        {
            //Arrange
            config.BaseUrl = $"https://{config.BaseUrl}";
            var getTestRequest = new GetTestRequest();
            var response = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.BadRequest
            };
            
            var httpMessageHandler = SetupMessageHandlerMock(response,new Uri(config.BaseUrl + getTestRequest.GetUrl), config.Key, HttpMethod.Get);
            var client = new HttpClient(httpMessageHandler.Object);
            var apiClient = new OuterApiClient(client, config);
            
            //Act Assert
            var actual = await apiClient.Get<List<string>>(getTestRequest);
            actual.StatusCode.Equals(HttpStatusCode.BadRequest);
            actual.Body.Should().BeNull();

        }
        
        [Test, AutoData]
        public async Task Then_If_It_Is_Not_Found_Default_Is_Returned(
            OuterApiConfiguration config)
        {
            //Arrange
            config.BaseUrl = $"https://{config.BaseUrl}";
            var getTestRequest = new GetTestRequest();
            var response = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.NotFound
            };
            
            var httpMessageHandler = SetupMessageHandlerMock(response, new Uri(config.BaseUrl + getTestRequest.GetUrl), config.Key, HttpMethod.Get);
            var client = new HttpClient(httpMessageHandler.Object);
            var apiClient = new OuterApiClient(client, config);
            
            //Act Assert
            var actual = await apiClient.Get<List<string>>(getTestRequest);

            actual.Body.Should().BeNull();
            actual.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private class GetTestRequest : IGetApiRequest
        {
            public string GetUrl => $"/test-url/get";
        }   
        
        private static Mock<HttpMessageHandler> SetupMessageHandlerMock(HttpResponseMessage response, Uri url, string key, HttpMethod httpMethod)
        {
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(c =>
                        c.Method.Equals(httpMethod)
                        && c.Headers.Contains("Ocp-Apim-Subscription-Key")
                        && c.Headers.GetValues("Ocp-Apim-Subscription-Key").First().Equals(key)
                        && c.Headers.Contains("X-Version")
                        && c.Headers.GetValues("X-Version").First().Equals("1")
                        && c.RequestUri.Equals(url)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync((HttpRequestMessage request, CancellationToken token) => response);
            return httpMessageHandler;
        }
    }
}