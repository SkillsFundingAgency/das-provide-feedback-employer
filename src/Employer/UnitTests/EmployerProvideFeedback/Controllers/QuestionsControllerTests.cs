using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using ESFA.DAS.EmployerProvideFeedback.Authentication;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Controllers;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using FluentAssertions;

namespace UnitTests.EmployerProvideFeedback.Controllers
{
    [TestFixture]
    public class QuestionsControllerTests
    {
        private QuestionsController _controller;
        private Mock<ISessionService> _sessionServiceMock;
        private IFixture _fixture;
        private List<ProviderAttributeModel> _providerAttributes;
        private string _accountId;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _providerAttributes = GetProviderAttributes();
            _sessionServiceMock = new Mock<ISessionService>();
            _sessionServiceMock
                .Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>()))
                .Returns(Task.FromResult(new SurveyModel()));

            var tempDataProvider = Mock.Of<ITempDataProvider>();
            var tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
            var tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

            _controller = new QuestionsController(_sessionServiceMock.Object)
            {
                TempData = tempData,
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                        new Claim(EmployerClaims.UserId, "TestUserIdValue"),
                    }))
                    }
                }
            };

            _accountId = string.Empty;
        }

        [Test]
        public async Task Question_1_When_No_Session_Answers_Should_Have_No_Doing_Well_Attributes()
        {
            // Act
            var result = await _controller.QuestionOne(_accountId) as ViewResult;

            // Assert
            result.Model.Should().BeOfType<SurveyModel>();
            var attributes = (result.Model as SurveyModel).Attributes;
            attributes.Should().NotContain(m => m.Good);
        }

        [Test]
        public async Task Question_1_When_Session_Answers_Should_Mark_As_Doing_Well()
        {
            // Arrange
            var surveyModel = new SurveyModel();
            var sessionDoingWellAtts = _providerAttributes.Take(3).ToList();
            sessionDoingWellAtts.ForEach(ps => ps.Good = true);
            surveyModel.Attributes = _providerAttributes;
            _sessionServiceMock.Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>()))
                .Returns(Task.FromResult(surveyModel));

            // Act
            var result = await _controller.QuestionOne(_accountId) as ViewResult;

            // Assert
            result.Model.Should().BeOfType<SurveyModel>();
            var attributes = (result.Model as SurveyModel).Attributes;
            attributes.Should().Contain(m => m.Good);
            attributes.Count(m => m.Good).Should().Be(sessionDoingWellAtts.Count);
        }

        [Test]
        public async Task Question_1_When_Answers_Submitted_Should_Update_Session_And_Redirect()
        {
            // Arrange
            var sessionDoingWellAtts = _providerAttributes.Take(3).ToList();
            sessionDoingWellAtts.ForEach(ps => ps.Good = true);
            var surveyModel = new SurveyModel { Attributes = _providerAttributes };

            // Act
            var result = await _controller.QuestionOne(surveyModel);

            // Assert
            _sessionServiceMock.Verify(mock => mock.Set(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            result.Should().BeOfType<RedirectToRouteResult>()
                .Which.RouteName.Should().Be(RouteNames.QuestionTwo_Get);
        }

        [Test]
        public async Task Question_1_Should_Handle_Return_Url()
        {
            // Arrange
            _controller.TempData.Add("ReturnUrl", RouteNames.ReviewAnswers_Get);

            // Act
            var result = await _controller.QuestionOne(new SurveyModel { Attributes = _providerAttributes });

            // Assert
            result.Should().BeOfType<RedirectToRouteResult>()
                .Which.RouteName.Should().Be(RouteNames.ReviewAnswers_Get);
        }

        [Test]
        public async Task Question_2_When_Q1_Skipped_Should_Have_No_Attributes_Doing_Well()
        {

            // Act
            var result = await _controller.QuestionTwo() as ViewResult;

            // Asserrt
            result.Model.Should().BeOfType<SurveyModel>();
            var attributes = (result.Model as SurveyModel).Attributes;
            attributes.Should().NotContain(m => m.Good);
        }

        [Test]
        public async Task Question_2_When_Q1_Skipped_And_Q2_Session_Answers_Should_Load_Previous_Selections()
        {
            // Arrange
            var surveyModel = new SurveyModel();
            var sessionDoingWellAtts = _providerAttributes.Take(3).ToList();
            sessionDoingWellAtts.ForEach(ps => ps.Bad = true);
            surveyModel.Attributes = _providerAttributes;
            _sessionServiceMock.Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>()))
                .Returns(Task.FromResult(surveyModel));

            // Act
            var result = await _controller.QuestionTwo() as ViewResult;

            // Assert
            result.Model.Should().BeOfType<SurveyModel>();
            var attributes = (result.Model as SurveyModel).Attributes;
            attributes.Should().Contain(m => m.Bad);
            attributes.Count(m => m.Bad).Should().Be(sessionDoingWellAtts.Count);
        }

        [Test]
        public async Task Question_2_When_Answers_Submitted_Should_Update_Session_And_Redirect()
        {
            // Arrange
            var sessionDoingWellAtts = _providerAttributes.Take(3).ToList();
            sessionDoingWellAtts.ForEach(ps => ps.Bad = true);
            var surveyModel = new SurveyModel { Attributes = _providerAttributes };

            // Act
            var result = await _controller.QuestionTwo(surveyModel);

            // Assert
            _sessionServiceMock.Verify(mock => mock.Set(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            result.Should().BeOfType<RedirectToRouteResult>()
                .Which.RouteName.Should().Be(RouteNames.QuestionThree_Get);
        }

        [Test]
        public async Task Question_2_Should_Handle_Return_Url()
        {
            // Arrange
            _controller.TempData.Add("ReturnUrl", RouteNames.ReviewAnswers_Get);

            // Act
            var result = await _controller.QuestionTwo(new SurveyModel { Attributes = _providerAttributes });

            // Assert
            result.Should().BeOfType<RedirectToRouteResult>()
                .Which.RouteName.Should().Be(RouteNames.ReviewAnswers_Get);
        }

        [Test]
        public async Task Question_3_When_Q1_And_Q2_Skipped_Should_Have_No_Selected_Attributes()
        {
            // Act
            var result = await _controller.QuestionThree() as ViewResult;

            // Assert
            result.Model.Should().BeOfType<SurveyModel>();
            var model = result.Model as SurveyModel;
            model.HasStrengths.Should().BeFalse();
            model.HasWeaknesses.Should().BeFalse();
        }

        [Test]
        public async Task Question_3_When_Q1_And_Q2_Skipped_And_Q3_Session_Answers_Should_Load_Previous_Selection()
        {
            // Arrange
            var surveyModel = new SurveyModel { Rating = ProviderRating.Poor };
            _sessionServiceMock.Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>()))
                .Returns(Task.FromResult(surveyModel));

            // Act
            var result = await _controller.QuestionThree() as ViewResult;

            // Assert
            result.Model.Should().BeOfType<SurveyModel>();
            var model = result.Model as SurveyModel;
            model.Rating.Should().Be(ProviderRating.Poor);
        }

        [Test]
        public async Task Question_3_When_Answer_Not_Selected_Should_Fail_Model_Validation()
        {
            // Arrange
            var surveyModel = new SurveyModel();
            _sessionServiceMock.Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>())).Verifiable();
            // simulate model validation as this only occurs at runtime
            _controller.ModelState.AddModelError("ProviderRating", "Required Field");

            // Act
            var result = await _controller.QuestionThree(surveyModel);

            // Assert
            _sessionServiceMock.Verify(mock => mock.Set(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            result.Should().BeOfType<ViewResult>();
        }

        [Test]
        public async Task Question_3_When_Answers_Submitted_Should_Update_Session_And_Redirect()
        {
            // Arrange
            var surveyModel = new SurveyModel { Rating = ProviderRating.Excellent };

            // Act
            var result = await _controller.QuestionThree(surveyModel) as RedirectToRouteResult;

            // Assert
            _sessionServiceMock.Verify(mock => mock.Set(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            result.Should().NotBeNull();
            result.RouteName.Should().Be(RouteNames.ReviewAnswers_Get);
        }

        [Test]
        public async Task Question_3_Should_Handle_Return_Url()
        {
            // Arrange
            var surveyModel = new SurveyModel { Rating = ProviderRating.Excellent };
            _controller.TempData.Add("ReturnUrl", RouteNames.ReviewAnswers_Get);

            // Act
            var result = await _controller.QuestionThree(surveyModel);

            // Assert
            result.Should().BeOfType<RedirectToRouteResult>()
                .Which.RouteName.Should().Be(RouteNames.ReviewAnswers_Get);
        }

        private List<ProviderAttributeModel> GetProviderAttributes()
        {
            return _fixture
                .Build<ProviderAttributeModel>()
                .With(x => x.Good, false)
                .With(x => x.Bad, false)
                .CreateMany(10)
                .ToList();
        }
    }
}
