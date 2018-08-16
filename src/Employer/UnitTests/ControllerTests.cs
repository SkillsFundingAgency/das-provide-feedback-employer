using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using ESFA.DAS.EmployerProvideFeedback.Controllers;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class ControllerTests
    {
        private QuestionsController _controller;
        private Mock<IHostingEnvironment> _hostingEnvMock;
        private Mock<ISessionService> _sessionServiceMock;
        private Mock<IOptions<List<ProviderSkill>>> _provSKillsOptions;
        private IFixture _fixture;
        private List<ProviderSkill> _providerSkills;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _hostingEnvMock = new Mock<IHostingEnvironment>();
            _sessionServiceMock = new Mock<ISessionService>();
            _provSKillsOptions = new Mock<IOptions<List<ProviderSkill>>>();

            _providerSkills = GetProviderSkills();
            _provSKillsOptions.SetupGet(mock => mock.Value).Returns(_providerSkills);
            _controller = new QuestionsController(_hostingEnvMock.Object, _sessionServiceMock.Object, _provSKillsOptions.Object);
        }

        [Test, Category("UnitTest")]
        public void When_No_Session_Answers_Should_Have_No_Doing_Well_Attributes()
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
        public void When_Session_Answers_Should_Mark_As_Doing_Well()
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
