using System;
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
        private readonly ISessionService _sessionService;
        private readonly AnswerModel _answerModel;

        public QuestionsController(ISessionService sessionService, IOptions<List<ProviderSkill>> providerSkills)
        {
            _sessionService = sessionService;
            _answerModel = new AnswerModel { ProviderSkills = providerSkills.Value };
        }

        [HttpGet("question-one", Name = RouteNames.QuestionOne_Get)]
        public IActionResult QuestionOne(Guid uniqueCode)
        {
            var cachedAnswers = _sessionService.Get<AnswerModel>(uniqueCode.ToString());
            return View(cachedAnswers != null ? cachedAnswers.ProviderSkills : _answerModel.ProviderSkills);
        }

        [HttpPost("question-one", Name = RouteNames.QuestionOne_Post)]
        public IActionResult QuestionOne(Guid uniqueCode, List<ProviderSkill> providerSkills)
        {
            var sessionAnswer = GetCurrentAnswerModel(uniqueCode);
            sessionAnswer.ProviderSkills = providerSkills;
            _sessionService.Set(uniqueCode.ToString(), sessionAnswer);

            return RedirectToRoute(RouteNames.QuestionTwo_Get);
        }

        [HttpGet("question-two", Name = RouteNames.QuestionTwo_Get)]
        public IActionResult QuestionTwo(Guid uniqueCode)
        {
            var sessionAnswers = GetCurrentAnswerModel(uniqueCode);
            return View(sessionAnswers.ProviderSkills);
        }

        [HttpPost("question-two", Name = RouteNames.QuestionTwo_Post)]
        public IActionResult QuestionTwo(Guid uniqueCode, List<ProviderSkill> providerSkills)
        {
            var sessionAnswer = GetCurrentAnswerModel(uniqueCode);
            sessionAnswer.ProviderSkills = providerSkills;
            _sessionService.Set(uniqueCode.ToString(), sessionAnswer);
            return RedirectToRoute(RouteNames.QuestionThree_Get);
        }

        [HttpGet("question-three", Name = RouteNames.QuestionThree_Get)]
        public IActionResult QuestionThree(Guid uniqueCode)
        {
            var sessionAnswer = GetCurrentAnswerModel(uniqueCode);
            return View(sessionAnswer);
        }

        public ViewResult QuestionOne(object uniqueCode)
        {
            throw new NotImplementedException();
        }

        [HttpPost("question-three", Name = RouteNames.QuestionThree_Post)]
        public IActionResult QuestionThree(Guid uniqueCode, AnswerModel answerModel)
        {
            if (!ModelState.IsValid)
            {
                return View(answerModel);
            }

            var sessionAnswer = GetCurrentAnswerModel(uniqueCode); 
            sessionAnswer.ProviderRating = answerModel.ProviderRating;
            _sessionService.Set(uniqueCode.ToString(), sessionAnswer);
            return RedirectToRoute(RouteNames.ReviewAnswers_Get);
        }

        private AnswerModel GetCurrentAnswerModel(Guid uniqueCode)
        {
            return _sessionService.Get<AnswerModel>(uniqueCode.ToString()) ?? _answerModel;
        }
    }
}
