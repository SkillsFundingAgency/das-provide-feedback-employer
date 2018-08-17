using System.Collections.Generic;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class QuestionsController : Controller
    {
        private const string SessionAnswerKey = "SessionAnswerKey";
        private readonly ISessionService _sessionService;
        private readonly AnswerModel _answerModel;

        public QuestionsController(ISessionService sessionService, IOptions<List<ProviderSkill>> providerSkills)
        {
            _sessionService = sessionService;
            _answerModel = new AnswerModel { ProviderSkills = providerSkills.Value };
        }

        [HttpGet("question-one", Name = RouteNames.QuestionOne_Get)]
        public IActionResult QuestionOne()
        {
            var cachedAnswers = _sessionService.Get<AnswerModel>(SessionAnswerKey);
            return View(cachedAnswers != null ? cachedAnswers.ProviderSkills : _answerModel.ProviderSkills);
        }

        [HttpPost("question-one", Name = RouteNames.QuestionOne_Post)]
        public IActionResult QuestionOne(List<ProviderSkill> providerSkills)
        {
            var sessionAnswer = GetCurrentAnswerModel();
            sessionAnswer.ProviderSkills = providerSkills;
            _sessionService.Set(SessionAnswerKey, sessionAnswer);

            return RedirectToRoute(RouteNames.QuestionTwo_Get);
        }

        [HttpGet("question-two", Name = RouteNames.QuestionTwo_Get)]
        public IActionResult QuestionTwo()
        {
            var sessionAnswers = GetCurrentAnswerModel();
            return View(sessionAnswers.ProviderSkills);
        }

        [HttpPost("question-two", Name = RouteNames.QuestionTwo_Post)]
        public IActionResult QuestionTwo(List<ProviderSkill> providerSkills)
        {
            var sessionAnswer = GetCurrentAnswerModel();
            sessionAnswer.ProviderSkills = providerSkills;
            _sessionService.Set(SessionAnswerKey, sessionAnswer);
            return RedirectToRoute(RouteNames.QuestionThree_Get);
        }

        [HttpGet("question-three", Name = RouteNames.QuestionThree_Get)]
        public IActionResult QuestionThree()
        {
            var sessionAnswer = GetCurrentAnswerModel();
            return View(sessionAnswer);
        }

        [HttpPost("question-three", Name = RouteNames.QuestionThree_Post)]
        public IActionResult QuestionThree(AnswerModel answerModel)
        {
            if (!ModelState.IsValid)
            {
                return View(answerModel);
            }

            var sessionAnswer = GetCurrentAnswerModel(); 
            sessionAnswer.ProviderRating = answerModel.ProviderRating;
            _sessionService.Set(SessionAnswerKey, sessionAnswer);
            return RedirectToRoute(RouteNames.ReviewAnswers_Get);
        }

        private AnswerModel GetCurrentAnswerModel()
        {
            return _sessionService.Get<AnswerModel>(SessionAnswerKey) ?? _answerModel;
        }
    }
}
