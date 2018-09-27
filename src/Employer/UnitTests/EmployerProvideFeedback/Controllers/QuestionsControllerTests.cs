using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Controllers;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace UnitTests.EmployerProvideFeedback.Controllers
{
    public class QuestionsControllerTests
    {
        private QuestionsController _controller;
        private Mock<ISessionService> _sessionServiceMock;
        private IFixture _fixture;
        private List<ProviderAttributeModel> _providerAttributes;
        private Guid _uniqueCode = Guid.NewGuid();

        public QuestionsControllerTests()
        {
            _fixture = new Fixture();
            _sessionServiceMock = new Mock<ISessionService>();
            _providerAttributes = GetProviderAttributes();
            _sessionServiceMock.Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>())).Returns(new SurveyModel());

            InitializeController();
        }

        private void InitializeController()
        {
            var httpContextMock = new Mock<HttpContext>();
            var tempDataProvider = new Mock<SessionStateTempDataProvider>();

            _controller = new QuestionsController(_sessionServiceMock.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContextMock.Object },
                TempData = new TempDataDictionary(httpContextMock.Object, tempDataProvider.Object)
            };
        }

        [Fact]
        public void Question_1_When_No_Session_Answers_Should_Have_No_Doing_Well_Attributes()
        {
            // Arrange

            // Act
            var result = _controller.QuestionOne(_uniqueCode) as ViewResult;

            // Assert
            Assert.IsAssignableFrom<SurveyModel>(result.Model);
            var attributes = (result.Model as SurveyModel).Attributes;
            Assert.DoesNotContain(attributes, m => m.Good);
        }

        [Fact]
        public void Question_1_When_Session_Answers_Should_Mark_As_Doing_Well()
        {
            // Arrange
            var surveyModel = new SurveyModel();
            var sessionDoingWellAtts = _providerAttributes.Take(3).ToList();
            sessionDoingWellAtts.ForEach(ps => ps.Good = true);
            surveyModel.Attributes = _providerAttributes;
            _sessionServiceMock.Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>())).Returns(surveyModel);

            // Act
            var result = _controller.QuestionOne(_uniqueCode) as ViewResult;

            // Assert
            Assert.IsAssignableFrom<SurveyModel>(result.Model);
            var attributes = (result.Model as SurveyModel).Attributes;
            Assert.Contains(attributes, m => m.Good);
            Assert.Equal(sessionDoingWellAtts.Count, attributes.Count(m => m.Good));
        }

        [Fact]
        public void Question_1_When_Answers_Submitted_Should_Update_Session_And_Redirect()
        {
            // Arrange
            var sessionDoingWellAtts = _providerAttributes.Take(3).ToList();
            sessionDoingWellAtts.ForEach(ps => ps.Good = true);
            var surveyModel = new SurveyModel { Attributes = _providerAttributes };

            // Act
            var result = _controller.QuestionOne(_uniqueCode, surveyModel);

            // Assert
            _sessionServiceMock.Verify(mock => mock.Set(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            Assert.IsAssignableFrom<RedirectToRouteResult>(result);
            Assert.Equal(RouteNames.QuestionTwo_Get, (result as RedirectToRouteResult).RouteName);
        }

        [Fact]
        public void Question_1_Should_Handle_Return_Url()
        {
            // Arrange
            _controller.TempData.Add("ReturnUrl", RouteNames.ReviewAnswers_Get);

            // Act
            var result = _controller.QuestionOne(_uniqueCode, new SurveyModel { Attributes = _providerAttributes });

            // Assert
            Assert.IsAssignableFrom<RedirectToRouteResult>(result);
            Assert.Equal(RouteNames.ReviewAnswers_Get, (result as RedirectToRouteResult).RouteName);
        }

        [Fact]
        public void Question_2_When_Q1_Skipped_Should_Have_No_Attributes_Doing_Well()
        {
            // Arrange

            // Act
            var result = _controller.QuestionTwo(_uniqueCode) as ViewResult;

            // Assert
            Assert.IsAssignableFrom<SurveyModel>(result.Model);
            var attributes = (result.Model as SurveyModel).Attributes;
            Assert.DoesNotContain(attributes, m => m.Good);
        }

        [Fact]
        public void Question_2_When_Q1_Skipped_And_Q2_Session_Answers_Should_Load_Previous_Selections()
        {
            // Arrange
            var surveyModel = new SurveyModel();
            var sessionDoingWellAtts = _providerAttributes.Take(3).ToList();
            sessionDoingWellAtts.ForEach(ps => ps.Bad = true);
            surveyModel.Attributes = _providerAttributes;
            _sessionServiceMock.Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>())).Returns(surveyModel);

            // Act
            var result = _controller.QuestionTwo(_uniqueCode) as ViewResult;

            // Assert
            Assert.IsAssignableFrom<SurveyModel>(result.Model);
            var attributes = (result.Model as SurveyModel).Attributes;
            Assert.Contains(attributes, m => m.Bad);
            Assert.Equal(sessionDoingWellAtts.Count, attributes.Count(m => m.Bad));
        }

        [Fact]
        public void Question_2_When_Answers_Submitted_Should_Update_Session_And_Redirect()
        {
            // Arrange
            var sessionDoingWellAtts = _providerAttributes.Take(3).ToList();
            sessionDoingWellAtts.ForEach(ps => ps.Bad = true);
            var surveyModel = new SurveyModel { Attributes = _providerAttributes };

            // Act
            var result = _controller.QuestionTwo(_uniqueCode, surveyModel);

            // Assert
            _sessionServiceMock.Verify(mock => mock.Set(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            Assert.IsAssignableFrom<RedirectToRouteResult>(result);
            Assert.Equal(RouteNames.QuestionThree_Get, (result as RedirectToRouteResult).RouteName);
        }

        [Fact]
        public void Question_2_Should_Handle_Return_Url()
        {
            // Arrange
            _controller.TempData.Add("ReturnUrl", RouteNames.ReviewAnswers_Get);

            // Act
            var result = _controller.QuestionTwo(_uniqueCode, new SurveyModel { Attributes = _providerAttributes });

            // Assert
            Assert.IsAssignableFrom<RedirectToRouteResult>(result);
            Assert.Equal(RouteNames.ReviewAnswers_Get, (result as RedirectToRouteResult).RouteName);
        }

        [Fact]
        public void Question_3_When_Q1_And_Q2_Skipped_Should_Have_No_Selected_Attributes()
        {
            // Arrange

            // Act
            var result = _controller.QuestionThree(_uniqueCode) as ViewResult;

            // Assert
            Assert.IsAssignableFrom<SurveyModel>(result.Model);
            var model = result.Model as SurveyModel;
            Assert.False(model.HasStrengths);
            Assert.False(model.HasWeaknesses);
        }

        [Fact]
        public void Question_3_When_Q1_And_Q2_Skipped_And_Q3_Session_Answers_Should_Load_Previous_Selection()
        {
            // Arrange
            var surveyModel = new SurveyModel();
            surveyModel.Rating = ProviderRating.Poor;
            _sessionServiceMock.Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>())).Returns(surveyModel);

            // Act
            var result = _controller.QuestionThree(_uniqueCode) as ViewResult;

            // Assert
            Assert.IsAssignableFrom<SurveyModel>(result.Model);
            var model = result.Model as SurveyModel;
            Assert.Equal(ProviderRating.Poor, model.Rating);
        }

        [Fact]
        public void Question_3_When_Answer_Not_Selected_Should_Fail_Model_Validation()
        {
            // Arrange
            var surveyModel = new SurveyModel();
            _sessionServiceMock.Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>())).Verifiable();

            // simulate model validation as this only occurs at runtime
            _controller.ModelState.AddModelError("ProviderRating", "Required Field");

            // Act
            var result = _controller.QuestionThree(_uniqueCode, surveyModel);

            // Assert
            _sessionServiceMock.Verify(mock => mock.Set(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            Assert.IsAssignableFrom<ViewResult>(result);
        }

        [Fact]
        public void Question_3_When_Answers_Submitted_Should_Update_Session_And_Redirect()
        {
            // Arrange
            var surveyModel = new SurveyModel { Rating = ProviderRating.Excellent };

            // Act
            var result = _controller.QuestionThree(_uniqueCode, surveyModel);

            // Assert
            _sessionServiceMock.Verify(mock => mock.Set(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            var redirectResult = Assert.IsAssignableFrom<RedirectToRouteResult>(result);
            Assert.Equal(RouteNames.ReviewAnswers_Get, redirectResult.RouteName);
        }

        [Fact]
        public void Question_3_Should_Handle_Return_Url()
        {
            // Arrange
            var surveyModel = new SurveyModel { Rating = ProviderRating.Excellent };
            _controller.TempData.Add("ReturnUrl", RouteNames.ReviewAnswers_Get);

            // Act
            var result = _controller.QuestionThree(_uniqueCode, surveyModel);

            // Assert
            Assert.IsAssignableFrom<RedirectToRouteResult>(result);
            Assert.Equal(RouteNames.ReviewAnswers_Get, (result as RedirectToRouteResult).RouteName);
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
