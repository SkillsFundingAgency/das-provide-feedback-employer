using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Controllers;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace UnitTests.Controllers
{
    [TestFixture]
    public class QuestionsControllerTests
    {
        private QuestionsController _controller;
        private Mock<ISessionService> _sessionServiceMock;
        private Mock<IOptions<List<ProviderSkill>>> _provSKillsOptions;
        private IFixture _fixture;
        private List<ProviderSkill> _providerSkills;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _sessionServiceMock = new Mock<ISessionService>();
            _provSKillsOptions = new Mock<IOptions<List<ProviderSkill>>>();

            _providerSkills = GetProviderSkills();
            _provSKillsOptions.SetupGet(mock => mock.Value).Returns(_providerSkills);
            _controller = new QuestionsController(_sessionServiceMock.Object, _provSKillsOptions.Object);
        }

        [Test, Category("UnitTest")]
        public void Question_1_When_No_Session_Answers_Should_Have_No_Doing_Well_Attributes()
        {
            // Arrange

            // Act
            var result = _controller.QuestionOne() as ViewResult;

            // Assert
            Assert.IsAssignableFrom<List<ProviderSkill>>(result.Model);
            var model = result.Model as List<ProviderSkill>;
            Assert.False(model.Any(m => m.IsDoingWell));
        }

        [Test, Category("UnitTest")]
        public void Question_1_When_Session_Answers_Should_Mark_As_Doing_Well()
        {
            // Arrange
            var answerModel = new AnswerModel();
            var sessionDoingWellSkills = _providerSkills.Take(3).ToList();
            sessionDoingWellSkills.ForEach(ps => ps.IsDoingWell = true);
            answerModel.ProviderSkills = _providerSkills;
            _sessionServiceMock.Setup(mock => mock.Get<AnswerModel>(It.IsAny<string>())).Returns(answerModel);

            // Act
            var result = _controller.QuestionOne() as ViewResult;

            // Assert
            Assert.IsAssignableFrom<List<ProviderSkill>>(result.Model);
            var model = result.Model as List<ProviderSkill>;
            Assert.True(model.Any(m => m.IsDoingWell));
            Assert.AreEqual(sessionDoingWellSkills.Count, model.Count(m => m.IsDoingWell));
        }

        [Test, Category("UnitTest")]
        public void Question_1_When_Answers_Submitted_Should_Update_Session_And_Redirect()
        {
            // Arrange
            var sessionDoingWellSkills = _providerSkills.Take(3).ToList();
            sessionDoingWellSkills.ForEach(ps => ps.IsDoingWell = true);

            // Act
            var result = _controller.QuestionOne(_providerSkills);

            // Assert
            _sessionServiceMock.Verify(mock => mock.Set(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            Assert.IsAssignableFrom<RedirectToRouteResult>(result);
            Assert.AreEqual(RouteNames.QuestionTwo_Get, (result as RedirectToRouteResult).RouteName);
        }

        [Test, Category("UnitTest")]
        public void Question_2_When_Q1_Skipped_Should_Have_No_Skills_Doing_Well()
        {
            // Arrange

            // Act
            var result = _controller.QuestionTwo() as ViewResult;

            // Assert
            Assert.IsAssignableFrom<List<ProviderSkill>>(result.Model);
            var model = result.Model as List<ProviderSkill>;
            Assert.False(model.Any(m => m.IsDoingWell));
        }

        [Test, Category("UnitTest")]
        public void Question_2_When_Q1_Skipped_And_Q2_Session_Answers_Should_Load_Previous_Selections()
        {
            // Arrange
            var answerModel = new AnswerModel();
            var sessionDoingWellSkills = _providerSkills.Take(3).ToList();
            sessionDoingWellSkills.ForEach(ps => ps.IsToImprove = true);
            answerModel.ProviderSkills = _providerSkills;
            _sessionServiceMock.Setup(mock => mock.Get<AnswerModel>(It.IsAny<string>())).Returns(answerModel);

            // Act
            var result = _controller.QuestionTwo() as ViewResult;

            // Assert
            Assert.IsAssignableFrom<List<ProviderSkill>>(result.Model);
            var model = result.Model as List<ProviderSkill>;
            Assert.True(model.Any(m => m.IsToImprove));
            Assert.AreEqual(sessionDoingWellSkills.Count, model.Count(m => m.IsToImprove));
        }

        [Test, Category("UnitTest")]
        public void Question_2_When_Answers_Submitted_Should_Update_Session_And_Redirect()
        {
            // Arrange
            var sessionDoingWellSkills = _providerSkills.Take(3).ToList();
            sessionDoingWellSkills.ForEach(ps => ps.IsToImprove = true);

            // Act
            var result = _controller.QuestionTwo(_providerSkills);

            // Assert
            _sessionServiceMock.Verify(mock => mock.Set(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            Assert.IsAssignableFrom<RedirectToRouteResult>(result);
            Assert.AreEqual(RouteNames.QuestionThree_Get, (result as RedirectToRouteResult).RouteName);
        }

        [Test, Category("UnitTest")]
        public void Question_3_When_Q1_And_Q2_Skipped_Should_Have_No_Selected_Skills()
        {
            // Arrange

            // Act
            var result = _controller.QuestionThree() as ViewResult;

            // Assert
            Assert.IsAssignableFrom<AnswerModel>(result.Model);
            var model = result.Model as AnswerModel;
            Assert.False(model.HasStrengths);
            Assert.False(model.HasWeaknesses);
        }

        [Test, Category("UnitTest")]
        public void Question_3_When_Q1_And_Q2_Skipped_And_Q3_Session_Answers_Should_Load_Previous_Selection()
        {
            // Arrange
            var answerModel = new AnswerModel();
            answerModel.ProviderRating = ProviderRating.Poor;
            _sessionServiceMock.Setup(mock => mock.Get<AnswerModel>(It.IsAny<string>())).Returns(answerModel);

            // Act
            var result = _controller.QuestionThree() as ViewResult;

            // Assert
            Assert.IsAssignableFrom<AnswerModel>(result.Model);
            var model = result.Model as AnswerModel;
            Assert.AreEqual(ProviderRating.Poor, model.ProviderRating);
        }

        [Test, Category("UnitTest")]
        public void Question_3_When_Answer_Not_Selected_Should_Fail_Model_Validation()
        {
            // Arrange
            var answerModel = new AnswerModel();
            _sessionServiceMock.Setup(mock => mock.Get<AnswerModel>(It.IsAny<string>())).Verifiable();

            // simulate model validation as this only occurs at runtime
            _controller.ModelState.AddModelError("ProviderRating", "Required Field");

            // Act
            var result = _controller.QuestionThree(answerModel);

            // Assert
            _sessionServiceMock.Verify(mock => mock.Set(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            Assert.IsNotAssignableFrom<RedirectToRouteResult>(result);
        }

        [Test, Category("UnitTest")]
        public void Question_3_When_Answers_Submitted_Should_Update_Session_And_Redirect()
        {
            // Arrange
            var answerModel = new AnswerModel { ProviderRating = ProviderRating.Excellent };

            // Act
            var result = _controller.QuestionThree(answerModel);

            // Assert
            _sessionServiceMock.Verify(mock => mock.Set(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            Assert.IsAssignableFrom<RedirectToRouteResult>(result);
            Assert.AreEqual(RouteNames.ReviewAnswers_Get, (result as RedirectToRouteResult).RouteName);
        }

        private List<ProviderSkill> GetProviderSkills()
        {
            return _fixture
                .Build<ProviderSkill>()
                .With(x => x.IsDoingWell, false)
                .With(x => x.IsToImprove, false)
                .CreateMany(10)
                .ToList();
        }
    }
}
